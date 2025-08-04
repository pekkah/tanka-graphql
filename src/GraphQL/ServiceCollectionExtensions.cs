using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using Tanka.GraphQL.SelectionSets;
using Tanka.GraphQL.Validation;

namespace Tanka.GraphQL;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDefaultTankaGraphQLServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IFieldCollector, DefaultFieldCollector>();
        services.AddKeyedSingleton<IDirectiveHandler>("skip", new SkipDirectiveHandler());
        services.AddKeyedSingleton<IDirectiveHandler>("include", new IncludeDirectiveHandler());
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

    /// <summary>
    /// Adds the @defer directive handler to enable incremental delivery
    /// </summary>
    public static IServiceCollection AddDeferDirective(this IServiceCollection services)
    {
        services.AddKeyedSingleton<IDirectiveHandler>("defer", new DeferDirectiveHandler());
        return services;
    }

    /// <summary>
    /// Adds the @stream directive handler to enable streaming of list fields
    /// </summary>
    public static IServiceCollection AddStreamDirective(this IServiceCollection services)
    {
        services.AddKeyedSingleton<IDirectiveHandler>("stream", new StreamDirectiveHandler());
        return services;
    }

    /// <summary>
    /// Adds both @defer and @stream directive handlers for incremental delivery
    /// </summary>
    public static IServiceCollection AddIncrementalDeliveryDirectives(this IServiceCollection services)
    {
        services.AddDeferDirective();
        services.AddStreamDirective();
        return services;
    }

    /// <summary>
    /// Adds a custom directive handler with the specified name
    /// </summary>
    /// <typeparam name="THandler">The type of directive handler</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="directiveName">The name of the directive (without @ symbol)</param>
    public static IServiceCollection AddDirectiveHandler<THandler>(this IServiceCollection services, string directiveName)
        where THandler : class, IDirectiveHandler
    {
        services.AddKeyedSingleton<IDirectiveHandler, THandler>(directiveName);
        return services;
    }

    /// <summary>
    /// Adds a custom directive handler instance with the specified name
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="directiveName">The name of the directive (without @ symbol)</param>
    /// <param name="handler">The directive handler instance</param>
    public static IServiceCollection AddDirectiveHandler(this IServiceCollection services, string directiveName, IDirectiveHandler handler)
    {
        services.AddKeyedSingleton<IDirectiveHandler>(directiveName, handler);
        return services;
    }
}