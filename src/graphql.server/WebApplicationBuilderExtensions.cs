using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Tanka.GraphQL.Server;

public static class WebApplicationBuilderExtensions
{
    public static GraphQLServiceBuilder AddTankaGraphQL3(this WebApplicationBuilder builder)
    {
        return builder.Services.AddTankaGraphQL3();
    }

    public static GraphQLServiceBuilder AddTankaGraphQL3(this IServiceCollection services)
    {
        return new GraphQLServiceBuilder(services);
    }
}