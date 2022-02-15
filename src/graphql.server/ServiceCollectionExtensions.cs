using System;
using Microsoft.Extensions.DependencyInjection;

namespace Tanka.GraphQL.Server;

public static class ServiceCollectionExtensions
{
    public static ServerBuilder AddTankaGraphQL(
        this IServiceCollection services, Action<ServerOptions> configure = null)
    {
        return new ServerBuilder(services, configure);
    }
}