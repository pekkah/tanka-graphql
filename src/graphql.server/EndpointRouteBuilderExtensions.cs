using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Routing;

namespace Tanka.GraphQL.Server;

public static class EndpointRouteBuilderExtensions
{
    public static HubEndpointConventionBuilder MapTankaGraphQLSignalR(
        this IEndpointRouteBuilder routes,
        string pattern,
        Action<HttpConnectionDispatcherOptions> configureOptions = null)
    {
        if (configureOptions != null)
            return routes.MapHub<ServerHub>(pattern, configureOptions);

        return routes.MapHub<ServerHub>(pattern);
    }

    public static IEndpointConventionBuilder MapTankaGraphQLWebSockets(
        this IEndpointRouteBuilder routes,
        string pattern)
    {
        var app = routes.CreateApplicationBuilder();
        app.UseTankaGraphQLWebSockets();

        return routes.Map(pattern, app.Build());
    }
}