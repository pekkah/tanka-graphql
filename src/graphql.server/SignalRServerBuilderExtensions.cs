using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Tanka.GraphQL.Extensions.Tracing;
using Tanka.GraphQL.Server.Links.DTOs;

namespace Tanka.GraphQL.Server
{
    public static class SignalRServerBuilderExtensions
    {
        /// <summary>
        ///     Add GraphQL query streaming hub with configured options
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static ISignalRServerBuilder AddTankaServerHub(
            this ISignalRServerBuilder builder)
        {
            var services = builder.Services;

            services.AddSignalR()
                .AddJsonProtocol(options =>
                {
                    options.PayloadSerializerOptions.Converters
                        .Add(new ObjectDictionaryConverter());
                });

            return builder;
        }
    }
}