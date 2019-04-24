using System;
using tanka.graphql.tracing;
using tanka.graphql.type;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace tanka.graphql.server
{
    public static class SignalRServerBuilderExtensions
    {
        public static ISignalRServerBuilder AddQueryStreamHubWithTracing(
            this ISignalRServerBuilder builder)
        {
            var services = builder.Services;

            // add tracing extension
            services.AddQueryExtension<TraceExtension>();

            // default configuration
            return AddQueryStreamHub(
                builder);
        }

        /// <summary>
        ///     Add GraphQL query streaming hub with default options.
        ///     Defaults:
        ///     - ISchema is resolved from <see cref="IServiceCollection" />
        ///     - IExtension's are resolved from <see cref="IServiceCollection" />
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static ISignalRServerBuilder AddQueryStreamHub(this ISignalRServerBuilder builder)
        {
            return AddQueryStreamHub(
                builder,
                optionsBuilder => optionsBuilder
                    .Configure<ISchema>((options, schema) => options.Schema = schema)
            );
        }

        /// <summary>
        ///     Add GraphQL query streaming hub with configured options
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static ISignalRServerBuilder AddQueryStreamHub(
            this ISignalRServerBuilder builder,
            Action<OptionsBuilder<QueryStreamHubOptions>> configureOptions)
        {
            var services = builder.Services;

            services.AddSignalR();
            var optionsBuilder = services.AddOptions<QueryStreamHubOptions>();
            configureOptions(optionsBuilder);

            services.TryAddTransient<QueryStreamService>();

            return builder;
        }
    }
}