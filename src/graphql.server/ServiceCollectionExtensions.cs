﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using tanka.graphql.extensions.tracing;
using tanka.graphql.server.webSockets;

namespace tanka.graphql.server
{
    public static class ServiceCollectionExtensions
    {
        public static OptionsBuilder<SchemaOptions> AddTankaSchemaOptions(
            this IServiceCollection services)
        {
            services.TryAddScoped<IQueryStreamService, QueryStreamService>();

            var optionsBuilder = services.AddOptions<SchemaOptions>();
            return optionsBuilder;
        }


        public static IServiceCollection AddTankaServerExecutionExtension<TExtension>(this IServiceCollection services)
            where TExtension : class, IExecutorExtension
        {
            services.TryAddSingleton<IExecutorExtension, TExtension>();

            return services;
        }

        public static OptionsBuilder<GraphQLWSProtocolOptions> AddTankaWebSocketServerWithTracing(
            this IServiceCollection services)
        {
            services.AddTankaServerExecutionExtension<TraceExtension>();
            return AddTankaWebSocketServer(services);
        }

        public static OptionsBuilder<GraphQLWSProtocolOptions> AddTankaWebSocketServer(this IServiceCollection services)
        {
            services.AddSingleton<WebSocketServer>();
            services.TryAddScoped<IProtocolHandler, GraphQLWSProtocol>();
            services.TryAddScoped<IMessageContextAccessor, MessageContextAccessor>();

            return services.AddOptions<GraphQLWSProtocolOptions>();
        }
    }
}