using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR;

namespace tanka.graphql.server
{
    public static class HubRouteBuilderExtensions
    {
        public static void MapTankaServerHub(
            this HubRouteBuilder routes,
            PathString route,
            Action<HttpConnectionDispatcherOptions> configureOptions = null)
        {
            if (configureOptions != null)
                routes.MapHub<ServerHub>(route, configureOptions);
            else
                routes.MapHub<ServerHub>(route);
        }
    }
}