using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Routing;

namespace Tanka.GraphQL.Server
{
    public static class EndpointRouteBuilderExtensions
    {
        public static HubEndpointConventionBuilder MapTankaGraphQLSignalR(
            this IEndpointRouteBuilder routes,
            PathString route,
            Action<HttpConnectionDispatcherOptions> configureOptions = null)
        {
            if (configureOptions != null)
                return routes.MapHub<ServerHub>(route, configureOptions);

            return routes.MapHub<ServerHub>(route);
        }
    }
}