using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.TypeSystem.ValueSerialization;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Extensions.ApolloFederation;

public record FederatedSchemaBuildOptions
{
    public SchemaBuildOptions? SchemaBuildOptions { get; init; }

    public IReferenceResolversMap? ReferenceResolvers { get; init; }
}

public static class Federation
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
        "_FieldSet"
    };

    private static IReadOnlyDictionary<string, IValueConverter> NoConverters { get; } =
        new Dictionary<string, IValueConverter>(0);

    private static object Service { get; } = new ();

    public static Task<ISchema> BuildSubgraph(this SchemaBuilder builder, FederatedSchemaBuildOptions options)
    {
        var schemaBuildOptions = options.SchemaBuildOptions ?? new SchemaBuildOptions();

        // query types entity types from builder (note that anything added after this wont' show up
        var entities = builder.QueryTypeDefinitions(type => type.HasDirective("key"), new SchemaBuildOptions
        {
            BuildTypesFromOrphanedExtensions = true
        }).ToList();

        // add federation types
        builder.Add(FederationTypes.TypeSystem);

        var resolvers = new ResolversMap(
            schemaBuildOptions.Resolvers ?? ResolversMap.None,
            schemaBuildOptions.Subscribers);

        // If no types are annotated with the key directive,
        // then the _Entity union and Query._entities field should be removed from the schema.
        if (entities.Any())
        {
            builder.Add(new TypeExtension(
                new UnionDefinition(
                    null,
                    "_Entity",
                    null,
                    new UnionMemberTypes(entities.Select(e => new NamedType(e.Name)).ToList()))
            ));

            builder.Add(new TypeExtension(
                new ObjectDefinition(null,
                    "Query",
                    fields: new FieldsDefinition(
                        new FieldDefinition[]
                        {
                            "_entities(representations: [_Any!]!): [_Entity]!",
                            "_service: _Service!"
                        }))));

            resolvers += new ResolversMap()
            {
                {"Query", "_service", _ =>  ResolveSync.As(Service)},
                {"Query", "_entities", CreateEntitiesResolver(options.ReferenceResolvers ?? new DictionaryReferenceResolversMap())},

                {"_Service", "sdl", CreateSdlResolver()}
            };
        }


        schemaBuildOptions = schemaBuildOptions with
        {
            ValueConverters = new Dictionary<string, IValueConverter>(schemaBuildOptions.ValueConverters ?? NoConverters)
            {
                { "_Any", new AnyScalarConverter() },
                { "_FieldSet", new FieldSetScalarConverter() }
            },
            Resolvers = resolvers,
            Subscribers = resolvers,
            BuildTypesFromOrphanedExtensions = true
        };      

        return builder.Build(schemaBuildOptions);
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
            /*var options = new SchemaPrinterOptions(context.ExecutionContext.Schema);
            var defaultShouldPrintType = options.ShouldPrintType;
            options.ShouldPrintType = type =>
            {
                if (type is DirectiveType directiveType)
                    if (IgnoredTypeNames.Contains(directiveType.Name))
                        return false;

                if (type is INamedType namedType)
                    if (IgnoredTypeNames.Contains(namedType.Name))
                        return false;

                if (type is ComplexType complexType)
                {
                    var fields = context.ExecutionContext.Schema
                        .GetFields(complexType.Name);

                    if (!fields.Any())
                        return false;
                }

                return defaultShouldPrintType(type);
            };

            var defaultShouldPrintField = options.ShouldPrintField;
            options.ShouldPrintField = (name, field) =>
            {
                if (name == "_service")
                    return false;

                if (name == "_entities")
                    return false;

                return defaultShouldPrintField(name, field);
            };

            var document = SchemaPrinter.Print(options);*/

            //todo: handle ignored types
            var schemaDefinition = context.Schema.ToTypeSystem();
            var sdl = Printer.Print(schemaDefinition);
            return ResolveSync.As(sdl);
        };
    }

    private static Resolver CreateEntitiesResolver(
        IReferenceResolversMap referenceResolversMap)
    {
        return async context =>
        {
            var representations = context
                .GetArgument<IReadOnlyCollection<object>>("representations");

            var result = new List<object>();
            var types = new Dictionary<object, TypeDefinition>();
            foreach (var representationObj in representations)
            {
                var representation = (IReadOnlyDictionary<string, object>)representationObj;
                if (!representation.TryGetValue("__typename", out var typenameObj))
                    throw new QueryExecutionException(
                        "Typename not found for representation",
                        context.Path,
                        context.Selection);

                var typename = typenameObj.ToString() ??
                               throw new QueryExecutionException(
                                   "Representation is missing __typename",
                                   context.Path,
                                   context.Selection);

                var objectType = context
                    .ExecutionContext
                    .Schema
                    .GetNamedType(typename) as ObjectDefinition;

                if (objectType == null)
                    throw new QueryExecutionException(
                        $"Could not resolve type from __typename: '{typename}'",
                        context.Path,
                        context.Selection);

                if (!referenceResolversMap.TryGetReferenceResolver(typename, out var resolveReference))
                    throw new QueryExecutionException(
                        $"Could not find reference resolvers for  __typename: '{typename}'",
                        context.Path,
                        context.Selection);

                var (namedType, reference) = await resolveReference(
                    context,
                    objectType,
                    representation);

                result.Add(reference);

                // this will fail if for same type there's multiple named types
                types.TryAdd(reference, namedType);
            }


            return Resolve.As(result, (_, reference) => types[reference]);
        };
    }
}