using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Tanka.GraphQL.Server.WebSockets;

namespace Tanka.GraphQL.Server
{
    public static class AppBuilderExtensions
    {
        public static IApplicationBuilder UseTankaGraphQLWebSockets(this IApplicationBuilder app)
        {
            var options = app.ApplicationServices.GetRequiredService<IOptions<WebSocketServerOptions>>();
            return UseTankaGraphQLWebSockets(app, options.Value);
        }

        public static IApplicationBuilder UseTankaGraphQLWebSockets(this IApplicationBuilder app,
            WebSocketServerOptions options)
        {
            app.Use(next => context =>
            {
                if (context.Request.Path.StartsWithSegments(options.Path)
                    && context.WebSockets.IsWebSocketRequest)
                {
                    var connection = context.RequestServices.GetRequiredService<WebSocketServer>();
                    return connection.ProcessRequestAsync(context);
                }

                return next(context);
            });

            return app;
        }
    }
}