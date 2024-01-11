using Microsoft.Extensions.DependencyInjection;

using Tanka.GraphQL.Validation;

namespace Tanka.GraphQL.Extensions.Experimental.OneOf;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOneOf(this IServiceCollection services)
    {
        services.PostConfigure<AsyncValidatorOptions>(
            options => options.Rules.Add(OneOfDirective.OneOfValidationRule())
        );

        return services;
    }
}