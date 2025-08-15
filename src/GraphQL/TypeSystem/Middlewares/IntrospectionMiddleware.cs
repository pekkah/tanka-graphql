using Tanka.GraphQL.Introspection;

namespace Tanka.GraphQL.TypeSystem;

public class IntrospectionMiddleware : ISchemaBuildMiddleware
{
    public async Task<ISchema> InvokeAsync(ISchemaBuildContext context, SchemaBuildDelegate next)
    {
        // Add introspection types and resolvers if requested
        if (context.Options.IncludeIntrospection)
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

        return await next(context);
    }
}