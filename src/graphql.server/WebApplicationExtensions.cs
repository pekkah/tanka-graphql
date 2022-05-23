using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Tanka.GraphQL.Server;

public static class WebApplicationExtensions
{
    public static IEndpointConventionBuilder MapTankaGraphQL3(
        this IEndpointRouteBuilder webApp,
        string pattern,
        string schemaName)
    {
        // resolve tanka application
        var tankaApp = webApp.ServiceProvider.GetRequiredService<GraphQLApplication>();
        return tankaApp.Map(pattern, schemaName, webApp);
    }
}