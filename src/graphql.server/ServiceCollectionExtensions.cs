using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using tanka.graphql.server.webSockets;
using tanka.graphql.tracing;

namespace tanka.graphql.server
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddQueryExtension<TExtension>(this IServiceCollection services)
            where TExtension: class, IExtension
        {
            services.TryAddSingleton<IExtension, TExtension>();

            return services;
        }

        public static IServiceCollection AddTankaWebSocketServerWithTracing(this IServiceCollection services)
        {
            services.AddQueryExtension<TraceExtension>();
            return AddTankaWebSocketServer(services);
        }

        public static IServiceCollection AddTankaWebSocketServer(this IServiceCollection services)
        {
            services.AddSingleton<WebSocketServer>();
            services.TryAddTransient<IProtocolHandler, GraphQLWSProtocol>();
            services.TryAddTransient<IQueryStreamService, QueryStreamService>();

            return services;
        }
    }
}