using Microsoft.Extensions.DependencyInjection;

namespace Tanka.GraphQL.Extensions.Experimental.OneOf;

public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Add @oneOf directive validation rule to default validator rules
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddOneOfValidationRule(this IServiceCollection services)
    {
        services.AddDefaultValidatorRule(OneOfDirective.OneOfValidationRule());
        return services;
    }
}