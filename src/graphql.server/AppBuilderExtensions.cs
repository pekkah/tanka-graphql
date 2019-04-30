using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using tanka.graphql.server.webSockets;

namespace tanka.graphql.server
{
    public static class AppBuilderExtensions
    {
        public static IApplicationBuilder UseTankaWebSocketServer(this IApplicationBuilder app)
        {
            var options = app.ApplicationServices.GetRequiredService<IOptions<WebSocketServerOptions>>();
            return UseTankaWebSocketServer(app, options.Value);
        }

        public static IApplicationBuilder UseTankaWebSocketServer(this IApplicationBuilder app,
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