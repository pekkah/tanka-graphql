using System;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using tanka.graphql.server.webSockets;
using tanka.graphql.tracing;

namespace tanka.graphql.server
{
    public static class ServiceCollectionExtensions
    {
        public static OptionsBuilder<ExecutionOptions> AddTankaExecutionOptions(
            this IServiceCollection services)
        {
            var optionsBuilder = services.AddOptions<ExecutionOptions>();
            return optionsBuilder;
        }


        public static IServiceCollection AddTankaServerExecutionExtension<TExtension>(this IServiceCollection services)
            where TExtension: class, IExtension
        {
            services.TryAddSingleton<IExtension, TExtension>();

            return services;
        }

        public static OptionsBuilder<GraphQLWSProtocolOptions> AddTankaWebSocketServerWithTracing(this IServiceCollection services)
        {
            services.AddTankaServerExecutionExtension<TraceExtension>();
            return AddTankaWebSocketServer(services);
        }

        public static OptionsBuilder<GraphQLWSProtocolOptions> AddTankaWebSocketServer(this IServiceCollection services)
        {
            services.AddSingleton<WebSocketServer>();
            services.TryAddScoped<IProtocolHandler, GraphQLWSProtocol>();
            services.TryAddScoped<IQueryStreamService, QueryStreamService>();
            services.TryAddScoped<IMessageContextAccessor, MessageContextAccessor>();

            return services.AddOptions<GraphQLWSProtocolOptions>();
        }
    }
}