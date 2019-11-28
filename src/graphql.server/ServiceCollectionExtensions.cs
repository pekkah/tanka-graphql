﻿using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Tanka.GraphQL.Extensions.Tracing;
using Tanka.GraphQL.Server.WebSockets;

namespace Tanka.GraphQL.Server
{
    public static class ServiceCollectionExtensions
    {
        public static OptionsBuilder<SchemaOptions> AddTankaSchemaOptions(
            this IServiceCollection services)
        {
            services.TryAddScoped<IQueryStreamService, QueryStreamService>();
            services.AddTankaServerExecutionExtension<ExecutionServiceScopeExtension>();

            var optionsBuilder = services.AddOptions<SchemaOptions>();
            return optionsBuilder;
        }


        public static IServiceCollection AddTankaServerExecutionExtension<TExtension>(this IServiceCollection services)
            where TExtension : class, IExecutorExtension
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IExecutorExtension, TExtension>());

            return services;
        }

        public static IServiceCollection AddTankaServerExecutionContextExtension<TContext>(
            this IServiceCollection services)
        {
            return services.AddTankaServerExecutionExtension<ContextExtension<TContext>>();
        }


        public static OptionsBuilder<WebSocketProtocolOptions> AddTankaWebSocketServer(this IServiceCollection services)
        {
            services.AddSingleton<WebSocketServer>();
            services.TryAddScoped<IProtocolHandler, GraphQLWSProtocol>();
            services.TryAddScoped<IMessageContextAccessor, MessageContextAccessor>();

            return services.AddOptions<WebSocketProtocolOptions>();
        }
    }
}