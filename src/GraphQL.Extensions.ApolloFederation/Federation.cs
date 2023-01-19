using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Extensions.ApolloFederation;

public record FederatedSchemaBuildOptions(IReferenceResolversMap ReferenceResolvers)
{
    public static FederatedSchemaBuildOptions Default = new(new DictionaryReferenceResolversMap());
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

    private static object Service { get; } = new();

    public static ExecutableSchemaBuilder AddFederation(this ExecutableSchemaBuilder builder,
        FederatedSchemaBuildOptions options)
    {
        builder.AddConfiguration(new FederationConfiguration(options));
        builder.AddValueConverter("_Any", new AnyScalarConverter());
        builder.AddValueConverter("_FieldSet", new FieldSetScalarConverter());

        return builder;
    }
}

public class FederationConfiguration : IExecutableSchemaConfiguration
{
    public FederationConfiguration(FederatedSchemaBuildOptions options)
    {
        Options = options;
    }

    public FederatedSchemaBuildOptions Options { get; }

    public Task Configure(SchemaBuilder schema, ResolversBuilder resolvers)
    {
        // query types entity types from builder (note that anything added after this wont' show up
        var entities = schema.QueryTypeDefinitions(type => type.HasDirective("key"), new()
        {
            BuildTypesFromOrphanedExtensions = true
        }).ToList();

        // add federation types
        schema.Add(FederationTypes.TypeSystem);

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

            /*resolvers += new ResolversMap
        {
            { "Query", "_service", _ => ResolveSync.As(Service) },
            {
                "Query", "_entities",
                CreateEntitiesResolver(options.ReferenceResolvers ?? new DictionaryReferenceResolversMap())
            },

            { "_Service", "sdl", CreateSdlResolver() }
        };*/
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
            var schemaDefinition = context.QueryContext.Schema.ToTypeSystem();
            var sdl = Printer.Print(schemaDefinition);
            context.ResolvedValue = sdl;

            return default;
        };
    }

    private static Resolver CreateEntitiesResolver(IReferenceResolversMap referenceResolversMap)
    {
        return async context =>
        {
            var representations = context
                .Arguments["representations"] as IReadOnlyCollection<object>;
            //.GetArgument<IReadOnlyCollection<object>>("representations");

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