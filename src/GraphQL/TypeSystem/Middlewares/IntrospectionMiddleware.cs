using System.Linq;
using System.Threading.Tasks;

using Tanka.GraphQL.Introspection;

namespace Tanka.GraphQL.TypeSystem;

public class IntrospectionMiddleware : ISchemaBuildMiddleware
{
    public async Task<ISchema> InvokeAsync(ISchemaBuildContext context, SchemaBuildDelegate next)
    {
        // Add introspection types and resolvers if requested and not already present
        if (context.Options.IncludeIntrospection)
        {
            var existingTypes = context.Builder.GetTypeDefinitions();
            var hasIntrospectionTypes = existingTypes.Any(t =>
                t.Name.Value == "__Schema" ||
                t.Name.Value == "__Type" ||
                t.Name.Value == "__Field");

            if (!hasIntrospectionTypes)
            {
                var (typeSystem, resolvers) = Introspect.Create();
                context.Builder.Add(typeSystem);

                foreach (var (typeName, fields) in resolvers.GetTypes())
                {
                    foreach (var fieldName in fields)
                    {
                        var resolver = resolvers.GetResolver(typeName, fieldName);
                        context.Resolvers.Resolver(typeName, fieldName).Run(resolver);
                    }
                }
            }
        }

        return await next(context);
    }
}