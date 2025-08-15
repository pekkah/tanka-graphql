using System.Threading.Tasks;

using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Extensions.ApolloFederation;

/// <summary>
/// Middleware that performs Federation initialization tasks
/// Runs in Initialization stage BEFORE LinkProcessingMiddleware
/// </summary>
public class FederationInitializationMiddleware : ISchemaBuildMiddleware
{
    private readonly SubgraphOptions _options;

    public FederationInitializationMiddleware(SubgraphOptions options)
    {
        _options = options;
    }

    public async Task<ISchema> InvokeAsync(ISchemaBuildContext context, SchemaBuildDelegate next)
    {
        // Currently no initialization needed - the schema should include its own @link directive
        // This middleware is kept for potential future initialization needs
        return await next(context);
    }
}