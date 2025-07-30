using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Tanka.GraphQL.Executable;
using Tanka.GraphQL.Fields;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Extensions.ApolloFederation;

public class SubgraphConfiguration : IExecutableSchemaConfiguration
{
    private static readonly IReadOnlyList<string> IgnoredTypeNames = new List<string>
    {
        "external",
        "requires",
        "provides",
        "key",
        "extends",
        "_Service",
        "_Entity",
        "_Any",
        "_FieldSet",
        "skip",
        "deprecated",
        "include"
    };

    public SubgraphConfiguration(SubgraphOptions options)
    {
        Options = options;
    }

    public SubgraphOptions Options { get; }

    public Task Configure(SchemaBuilder schema, ResolversBuilder resolvers)
    {
        // query entity types from builder (note that anything added after this wont' show up)
        var entities = schema.QueryTypeDefinitions(type => type.HasDirective("key"), new()
        {
            BuildTypesFromOrphanedExtensions = true
        }).ToList();

        // add federation types
        schema.Add(SubgraphTypes.TypeSystem);

        // If no types are annotated with the key directive,
        // then the _Entity union and Query._entities field should be removed from the schema.
        if (entities.Any())
        {
            schema.Add(new TypeExtension(
                new UnionDefinition(
                    null,
                    "_Entity",
                    null,
                    new(entities.Select(e => new NamedType(e.Name)).ToList()))
            ));

            schema.Add(new TypeExtension(
                new ObjectDefinition(null,
                    "Query",
                    fields: new(
                        new FieldDefinition[]
                        {
                            "_entities(representations: [_Any!]!): [_Entity]!",
                            "_service: _Service!"
                        }))));

            resolvers.Resolver("Query", "_service").ResolveAs("Service");
            resolvers.Resolver("Query", "_entities").Run(CreateEntitiesResolver(Options.ReferenceResolvers));
            resolvers.Resolver("_Service", "sdl").Run(CreateSdlResolver());
        }

        return Task.CompletedTask;
    }

    private static IReadOnlyList<NamedType> GetEntities(ISchema schema)
    {
        return schema.QueryTypes<ObjectDefinition>()
            .Where(obj => obj.HasDirective("key"))
            .Select(entity => new NamedType(entity.Name, null))
            .ToList();
    }

    private static Resolver CreateSdlResolver()
    {
        return context =>
        {
            var schemaDefinition = context.QueryContext.Schema.ToTypeSystem();
            var sdl = Printer.Print(schemaDefinition, node =>
            {
                if (node is DirectiveDefinition directiveType)
                    if (IgnoredTypeNames.Contains(directiveType.Name.Value) ||
                        directiveType.Name.Value.StartsWith("__"))
                        return false;

                if (node is ObjectDefinition namedType)
                    if (IgnoredTypeNames.Contains(namedType.Name.Value) || namedType.Name.Value.StartsWith("__") ||
                        namedType.Fields?.Any() == false)
                        return false;

                if (node is ObjectDefinition queryType)
                    if (queryType.Name.Value == "Query" &&
                        queryType.Fields?.Where(f => !f.Name.Value.StartsWith("_")).Any() == false)
                        return false;

                if (node is InterfaceDefinition interfaceDefinition)
                    if (IgnoredTypeNames.Contains(interfaceDefinition.Name.Value) ||
                        interfaceDefinition.Name.Value.StartsWith("__"))
                        return false;

                if (node is UnionDefinition unionDefinition)
                    if (IgnoredTypeNames.Contains(unionDefinition.Name.Value) ||
                        unionDefinition.Name.Value.StartsWith("__"))
                        return false;

                if (node is FieldDefinition fieldDefinition)
                    if (new[] { "_service", "_entities", "__type", "__schema" }.Contains(fieldDefinition.Name.Value))
                        return false;

                if (node is ScalarDefinition scalarDefinition)
                    if (Scalars.Standard.Any(standard => standard.Type.Name.Value == scalarDefinition.Name.Value) ||
                        IgnoredTypeNames.Contains(scalarDefinition.Name.Value))
                        return false;

                if (node is EnumDefinition enumDefinition)
                    if (IgnoredTypeNames.Contains(enumDefinition.Name.Value) ||
                        enumDefinition.Name.Value.StartsWith("__"))
                        return false;

                if (node is SchemaDefinition)
                    return false;


                return true;
            }, printDescriptions: false /* todo: should we include descriptions*/);
            context.ResolvedValue = sdl;

            return default;
        };
    }


    private static Resolver CreateEntitiesResolver(IReferenceResolversMap referenceResolversMap)
    {
        return async context =>
        {
            var representations = context
                .GetArgument<IReadOnlyCollection<object>>("representations");

            ArgumentNullException.ThrowIfNull(representations);

            var result = new List<object>();
            var types = new Dictionary<object, TypeDefinition>();
            foreach (var representationObj in representations)
            {
                var representation = (IReadOnlyDictionary<string, object>)representationObj;
                if (!representation.TryGetValue("__typename", out var typenameObj))
                    throw new QueryException(
                        "Typename not found for representation")
                    {
                        Path = context.Path
                    };

                var typename = typenameObj.ToString() ??
                               throw new QueryException(
                                   "Representation is missing __typename")
                               {
                                   Path = context.Path
                               };

                var objectType = context
                    .QueryContext
                    .Schema
                    .GetNamedType(typename) as ObjectDefinition;

                if (objectType == null)
                    throw new QueryException(
                        $"Could not resolve type from __typename: '{typename}'")
                    {
                        Path = context.Path
                    };

                if (!referenceResolversMap.TryGetReferenceResolver(typename, out var resolveReference))
                    throw new QueryException(
                        $"Could not find reference resolvers for  __typename: '{typename}'")
                    {
                        Path = context.Path
                    };

                var (namedType, reference) = await resolveReference(
                    context,
                    objectType,
                    representation);

                result.Add(reference);

                // this will fail if for same type there's multiple named types
                types.TryAdd(reference, namedType);
            }

            context.ResolvedValue = result;
            context.ResolveAbstractType = (_, o) =>
                types[o ?? throw new ArgumentNullException(nameof(o), "Cannot resolve actual type")];
        };
    }
}