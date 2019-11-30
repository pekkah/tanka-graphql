using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Tanka.GraphQL.Server.WebSockets;

namespace Tanka.GraphQL.Server
{
    public static class AppBuilderExtensions
    {
        public static IApplicationBuilder UseTankaGraphQLWebSockets(this IApplicationBuilder app,
            PathString path)
        {
            app.Use(next => context =>
            {
                if (context.Request.Path.StartsWithSegments(path)
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