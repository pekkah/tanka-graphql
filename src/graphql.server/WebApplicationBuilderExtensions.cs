using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Tanka.GraphQL.Server;

public static class WebApplicationBuilderExtensions
{
    public static GraphQLApplicationBuilder AddTankaGraphQL(this WebApplicationBuilder builder)
    {
        return builder.Services.AddTankaGraphQL();
    }

    public static GraphQLApplicationBuilder AddTankaGraphQL(this IServiceCollection services)
    {
        return new GraphQLApplicationBuilder(services);
    }
}