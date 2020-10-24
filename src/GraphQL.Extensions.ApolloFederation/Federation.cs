using System.Collections.Generic;
using System.Linq;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.SDL;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Extensions.ApolloFederation
{
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

        public static SchemaBuilder ServiceFrom(
            ISchema schema,
            IReferenceResolversMap referenceResolvers)
        {
            var builder = new SchemaBuilder()
                .Import(schema);

            builder.Union("_Entity", out var entityUnion, possibleTypes: GetEntities(builder));
            builder.Object("_Service", out var serviceObject);

            if (!builder.TryGetQuery(out var queryObject))
                builder.Query(out queryObject);

            var entitiesResolver = CreateEntitiesResolver(referenceResolvers);
            builder.Connections(connect =>
            {
                connect.Field(
                    queryObject,
                    "_entities",
                    new NonNull(new List(entityUnion)),
                    args: args =>
                    {
                        args.Arg(
                            "representations",
                            new NonNull(new List(new NonNull(FederationTypes._Any))),
                            null,
                            "Representations");
                    },
                    resolve: resolve => resolve.Run(entitiesResolver));

                connect.Field(
                    queryObject,
                    "_service",
                    new NonNull(serviceObject),
                    "Federation",
                    resolve => resolve.Run(context => ResolveSync.As(new object())));

                connect.Field(
                    serviceObject,
                    "sdl",
                    ScalarType.NonNullString,
                    "SDL",
                    resolve => resolve.Run(CreateSdlResolver()));
            });

            return builder;
        }

        private static ObjectType[] GetEntities(SchemaBuilder builder)
        {
            return builder.GetTypes<ObjectType>()
                .Where(obj => obj.HasDirective("key"))
                .ToArray();
        }

        private static Resolver CreateSdlResolver()
        {
            return context =>
            {
                var options = new SchemaPrinterOptions(context.ExecutionContext.Schema);
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

                var document = SchemaPrinter.Print(options);
                var sdl = Printer.Print(document);
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
                var types = new Dictionary<object, INamedType>();
                foreach (var representationObj in representations)
                {
                    var representation = (IReadOnlyDictionary<string, object>) representationObj;
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
                        .GetNamedType<ObjectType>(typename);

                    if (objectType == null)
                        throw new QueryExecutionException(
                            $"Could not resolve type form __typename: '{typename}'",
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

                return Resolve.As(result, reference => types[reference]);
            };
        }
    }
}