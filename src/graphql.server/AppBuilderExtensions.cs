using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Tanka.GraphQL.Server.WebSockets;

namespace Tanka.GraphQL.Server;

public static class AppBuilderExtensions
{
    [Obsolete("UseEndpoints(endpoints => endpoints.MapTankaGraphQLWebSockets(\"path\"))")]
    public static IApplicationBuilder UseTankaGraphQLWebSockets(
        this IApplicationBuilder app,
        PathString path)
    {
        app.Use(next => context =>
        {
            if (context.Request.Path.StartsWithSegments(path)
                && context.WebSockets.IsWebSocketRequest)
            {
                var server = context.RequestServices.GetRequiredService<WebSocketServer>();
                return server.ProcessRequestAsync(context);
            }

            return next(context);
        });

        return app;
    }

    public static IApplicationBuilder UseTankaGraphQLWebSockets(
        this IApplicationBuilder app)
    {
        app.Use(next => context =>
        {
            if (!context.WebSockets.IsWebSocketRequest)
                return next(context);

            var server = context.RequestServices.GetRequiredService<WebSocketServer>();
            return server.ProcessRequestAsync(context);
        });

        return app;
    }
}