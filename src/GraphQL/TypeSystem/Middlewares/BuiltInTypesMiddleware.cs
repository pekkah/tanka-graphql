using System.Threading.Tasks;

using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.TypeSystem;

/// <summary>
/// Middleware that handles type collection and organization
/// </summary>
public class BuiltInTypesMiddleware : ISchemaBuildMiddleware
{
    public async Task<ISchema> InvokeAsync(ISchemaBuildContext context, SchemaBuildDelegate next)
    {
        var options = context.Options;

        // Add built-in types if requested
        if (options.IncludeBuiltInTypes)
        {
            context.Builder.Add(SchemaBuilder.BuiltInTypes);
        }

        return await next(context);
    }
}