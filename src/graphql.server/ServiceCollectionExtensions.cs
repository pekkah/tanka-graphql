using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace tanka.graphql.server
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddQueryExtension<TExtension>(this IServiceCollection services)
            where TExtension: class, IExtension
        {
            services.TryAddTransient<IExtension, TExtension>();

            return services;
        }
    }
}