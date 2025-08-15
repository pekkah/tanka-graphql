using System.Threading.Tasks;

namespace Tanka.GraphQL.TypeSystem;

/// <summary>
/// Middleware that processes @link directives and imports schemas
/// </summary>
public class LinkProcessingMiddleware : ISchemaBuildMiddleware
{
    public async Task<ISchema> InvokeAsync(ISchemaBuildContext context, SchemaBuildDelegate next)
    {
        var options = context.Options;

        // Process @link directives if SchemaLoader is available
        if (context.SchemaLoader != null)
        {
            await context.Builder.ProcessLinkDirectivesAsync(options);
        }

        return await next(context);
    }
}