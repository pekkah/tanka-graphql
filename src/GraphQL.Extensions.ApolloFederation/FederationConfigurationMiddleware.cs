using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tanka.GraphQL.Fields;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Extensions.ApolloFederation;

/// <summary>
/// Middleware that configures Apollo Federation resolvers after @link processing is complete
/// Runs in TypeResolution stage AFTER LinkProcessingMiddleware has imported Federation types
/// </summary>
public class FederationConfigurationMiddleware : ISchemaBuildMiddleware
{
    private readonly SubgraphOptions _options;

    public FederationConfigurationMiddleware(SubgraphOptions options)
    {
        _options = options;
    }

    public async Task<ISchema> InvokeAsync(ISchemaBuildContext context, SchemaBuildDelegate next)
    {
        // Check if Federation types are available in the schema
        // If not, skip configuration as the schema doesn't use Federation
        if (!HasFederationSupport(context))
        {
            return await next(context);
        }

        // Configure Federation resolvers (types already imported via @link)
        ConfigureFederationResolvers(context);

        return await next(context);
    }

    private bool HasFederationSupport(ISchemaBuildContext context)
    {
        // Check if key Federation directives exist
        var directives = context.Builder.GetDirectiveDefinitions();
        return directives.Any(d => d.Name == "key" || d.Name == "external" || d.Name == "requires");
    }

    private void ConfigureFederationResolvers(ISchemaBuildContext context)
    {
        // Query entity types from builder (Federation types should already be imported via @link)
        var entities = context.Builder.GetTypeDefinitions(context.Options.BuildTypesFromOrphanedExtensions)
            .Where(type => type.HasDirective("key"))
            .ToList();

        // Add _service field and resolver
        AddServiceField(context.Builder, context.Resolvers);

        // Add _entities field and resolver if there are entities
        if (entities.Count > 0)
        {
            AddEntitiesField(context, context.Builder, context.Resolvers, entities);
        }
    }

    /// <summary>
    /// Add _service field to Query type
    /// </summary>
    private void AddServiceField(SchemaBuilder schema, ResolversBuilder resolvers)
    {
        // Add _service field to Query
        schema.Add(new TypeExtension(
            new ObjectDefinition(null,
                "Query",
                fields: new(
                    new FieldDefinition[]
                    {
                        "_service: _Service!"
                    }))));

        // Add resolver that returns a service object
        resolvers.Resolver("Query", "_service").Run(context =>
        {
            var schema = context.QueryContext.Schema;
            var sdl = GenerateSchemaSDL(schema);
            context.ResolvedValue = new { sdl };
            return ValueTask.CompletedTask;
        });

        // Add resolver for the sdl field on _Service type
        resolvers.Resolver("_Service", "sdl").Run(context =>
        {
            // The object should already have the sdl property from the parent resolver
            if (context.ObjectValue is { } obj)
            {
                var sdlProperty = obj.GetType().GetProperty("sdl");
                if (sdlProperty != null)
                {
                    context.ResolvedValue = sdlProperty.GetValue(obj);
                    return ValueTask.CompletedTask;
                }
            }

            // Fallback: generate SDL from schema
            var schema = context.QueryContext.Schema;
            context.ResolvedValue = GenerateSchemaSDL(schema);
            return ValueTask.CompletedTask;
        });
    }


    /// <summary>
    /// Add _entities field to Query type for entity resolution
    /// </summary>
    private void AddEntitiesField(ISchemaBuildContext context, SchemaBuilder schema, ResolversBuilder resolvers, IReadOnlyList<TypeDefinition> entities)
    {
        // Add union of all entity types only if it doesn't already exist (from @link import)
        var entityNames = entities.Select(e => e.Name.Value).ToList();
        var unionMembers = string.Join(" | ", entityNames);

        var existingTypes = schema.GetTypeDefinitions(context.Options.BuildTypesFromOrphanedExtensions);
        if (!existingTypes.Any(t => t.Name.Value == "_Entity"))
        {
            schema.Add($"union _Entity = {unionMembers}");
        }

        // Add _entities field to Query
        schema.Add(new TypeExtension(
            new ObjectDefinition(null,
                "Query",
                fields: new(
                    new FieldDefinition[]
                    {
                        "_entities(representations: [_Any!]!): [_Entity]!"
                    }))));

        // Add resolver that uses the configured reference resolvers
        resolvers.Resolver("Query", "_entities").Run(CreateEntitiesResolver(_options.ReferenceResolvers, entities));
    }


    /// <summary>
    /// Create the entities resolver that handles Federation entity resolution
    /// </summary>
    private static Resolver CreateEntitiesResolver(IReferenceResolversMap referenceResolversMap, IReadOnlyList<TypeDefinition> entities)
    {
        return async context =>
        {
            // Set up abstract type resolver for _Entity union
            context.ResolveAbstractType = (abstractType, value) =>
            {
                if (value == null)
                    return null;

                // If the object has a __typename property, use that
                if (value is Dictionary<string, object> dict)
                {
                    if (dict.TryGetValue("__typename", out var typename) && typename is string type)
                        return context.QueryContext.Schema.GetNamedType(type);
                }

                // Look for a matching type by checking if the object is one of the entity types
                var objectType = value.GetType();
                foreach (var entity in entities)
                {
                    // Try to match by type name (simple heuristic)
                    if (objectType.Name.Equals(entity.Name.Value, StringComparison.OrdinalIgnoreCase))
                        return entity;
                }

                // If we can't resolve the type, throw a descriptive error
                // This prevents undefined behavior and silent failures in Federation
                throw new InvalidOperationException(
                    $"Unable to resolve GraphQL type for object of type '{objectType.Name}'. " +
                    $"Available entity types: [{string.Join(", ", entities.Select(e => e.Name.Value))}]. " +
                    "Ensure the object has a '__typename' property or the reference resolver returns the correct type.");
            };

            var representations = context
                .GetArgument<IReadOnlyCollection<object>>("representations");
            ArgumentNullException.ThrowIfNull(representations);

            var result = new List<object?>();

            foreach (var representation in representations)
            {
                if (representation is not Dictionary<string, object> dict)
                    continue;

                if (!dict.TryGetValue("__typename", out var typeNameObj)
                    || typeNameObj is not string typeName)
                    continue;

                if (!referenceResolversMap.TryGetReferenceResolver(typeName, out var referenceResolver))
                    continue;

                var typeDefinition = context.QueryContext.Schema.GetNamedType(typeName);
                if (typeDefinition is not ObjectDefinition objectDefinition)
                    continue;

                var resolveResult = await referenceResolver(context, objectDefinition, dict);
                result.Add(resolveResult.Reference);
            }

            context.ResolvedValue = result;
        };
    }

    /// <summary>
    /// Generate SDL representation for Federation subgraph schema
    /// </summary>
    private static string GenerateSchemaSDL(ISchema schema)
    {
        // Convert schema to TypeSystemDocument and print as SDL
        var document = schema.ToTypeSystem();

        // Filter nodes according to Apollo Federation spec:
        // Include user-defined types and fields, exclude built-ins and introspection
        // Don't print descriptions as they're not expected in Federation subgraph SDL
        return Language.Printer.Print(document,
            node => ShouldIncludeNodeInSubgraphSDL(node),
            printDescriptions: false);
    }

    /// <summary>
    /// Determines if a node should be included in the subgraph SDL according to Apollo Federation spec
    /// </summary>
    private static bool ShouldIncludeNodeInSubgraphSDL(INode node)
    {
        return node switch
        {
            // For object types, only include entity types (exclude Query, Mutation, Subscription)
            ObjectDefinition obj => ShouldIncludeObjectType(obj),

            // Include user-defined interfaces, unions, enums, input types (exclude built-ins)
            InterfaceDefinition iface => !IsBuiltInOrIntrospectionType(iface.Name.Value),
            UnionDefinition union => !IsBuiltInOrIntrospectionType(union.Name.Value),
            EnumDefinition enumDef => !IsBuiltInOrIntrospectionType(enumDef.Name.Value),
            InputObjectDefinition input => !IsBuiltInOrIntrospectionType(input.Name.Value),

            // Exclude built-in scalar types and federation scalars
            ScalarDefinition scalar => !IsBuiltInScalarType(scalar.Name.Value) && !IsApolloFederationBuiltInType(scalar.Name.Value),

            // Exclude all directive definitions from subgraph SDL
            DirectiveDefinition => false,

            // Exclude schema definition as it's not needed for subgraph SDL
            SchemaDefinition => false,

            // Include everything else (field definitions, arguments, etc.)
            _ => true
        };
    }

    /// <summary>
    /// Determine if an object type should be included in the subgraph SDL
    /// Only include entity types, exclude root operation types per Apollo Federation spec
    /// </summary>
    private static bool ShouldIncludeObjectType(ObjectDefinition obj)
    {
        // Exclude built-in and introspection types
        if (IsBuiltInOrIntrospectionType(obj.Name.Value))
            return false;

        // Exclude root operation types (Query, Mutation, Subscription) from federation SDL
        if (obj.Name.Value is "Query" or "Mutation" or "Subscription")
            return false;

        // Include all other user-defined object types (entities)
        return true;
    }

    /// <summary>
    /// Check if a type name is a built-in GraphQL scalar, introspection type, or Apollo Federation built-in type
    /// </summary>
    private static bool IsBuiltInOrIntrospectionType(string typeName)
    {
        return IsBuiltInScalarType(typeName) || IsIntrospectionType(typeName) || IsApolloFederationBuiltInType(typeName);
    }

    /// <summary>
    /// Check if a type name is an Apollo Federation built-in type that should be excluded from subgraph SDL
    /// </summary>
    private static bool IsApolloFederationBuiltInType(string typeName)
    {
        return typeName is "_Any" or "_Entity" or "_Service" or "FieldSet" or "link__Import" or "link__Purpose";
    }

    /// <summary>
    /// Check if a type name is a built-in GraphQL scalar type
    /// </summary>
    private static bool IsBuiltInScalarType(string typeName)
    {
        return typeName is "String" or "Int" or "Float" or "Boolean" or "ID";
    }

    /// <summary>
    /// Check if a type name is a GraphQL introspection type
    /// </summary>
    private static bool IsIntrospectionType(string typeName)
    {
        return typeName.StartsWith("__");
    }


}