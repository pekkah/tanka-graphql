using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Tanka.GraphQL.Server;

public static class WebApplicationBuilderExtensions
{
    public static GraphQLApplicationBuilder AddTankaGraphQL3(this WebApplicationBuilder builder)
    {
        return builder.Services.AddTankaGraphQL3();
    }

    public static GraphQLApplicationBuilder AddTankaGraphQL3(this IServiceCollection services)
    {
        return new GraphQLApplicationBuilder(services);
    }
}