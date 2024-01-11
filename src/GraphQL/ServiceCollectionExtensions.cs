using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using Tanka.GraphQL.Validation;

namespace Tanka.GraphQL;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDefaultTankaGraphQLServices(this IServiceCollection services)
    {
        return services.AddDefaultValidator();
    }
    
    public static IServiceCollection AddDefaultValidator(this IServiceCollection services)
    {
        services.AddOptions();
        services.AddOptions<AsyncValidatorOptions>();
        services.TryAddSingleton<IAsyncValidator>(p =>
            new AsyncValidator(p.GetRequiredService<IOptions<AsyncValidatorOptions>>()));
        
        return services;
    }

    public static IServiceCollection AddDefaultValidatorRule(this IServiceCollection services, CombineRule rule)
    {
        services.PostConfigure<AsyncValidatorOptions>(options => options.Rules.Add(rule));
        return services;
    }
}