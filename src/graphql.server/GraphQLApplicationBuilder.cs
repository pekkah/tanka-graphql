using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Tanka.GraphQL.Validation;

namespace Tanka.GraphQL.Server;

public class GraphQLApplicationBuilder
{
    public GraphQLApplicationBuilder(IServiceCollection applicationServices)
    {
        ApplicationServices = applicationServices;
        ApplicationOptionsBuilder = ApplicationServices
            .AddOptions<GraphQLApplicationOptions>();

        AddCore();
    }

    public IServiceCollection ApplicationServices { get; }

    public OptionsBuilder<GraphQLApplicationOptions> ApplicationOptionsBuilder { get; }

    public GraphQLApplicationBuilder AddHttp()
    {
        ApplicationServices.TryAddEnumerable(ServiceDescriptor.Singleton<IGraphQLTransport, GraphQLHttpTransport>());
        return this;
    }

    public GraphQLApplicationBuilder AddSchema(
        string schemaName,
        Action<SchemaOptionsBuilder> configureOptions)
    {
        OptionsBuilder<SchemaOptions> schemaOptions = ApplicationServices
            .AddOptions<SchemaOptions>(schemaName);

        var optionsBuilder = new SchemaOptionsBuilder(
            schemaOptions,
            ApplicationServices);

        configureOptions(optionsBuilder);

        ApplicationOptionsBuilder.Configure(options => options.SchemaNames.Add(schemaName));

        return this;
    }

    public GraphQLApplicationBuilder AddWebSockets()
    {
        ApplicationServices.TryAddEnumerable(ServiceDescriptor.Singleton<IGraphQLTransport, GraphQLWSTransport>());
        return this;
    }

    private void AddCore()
    {
        ApplicationServices.TryAddSingleton<IHostedService, SchemaInitializer>();
        ApplicationServices.TryAddSingleton<SchemaCollection>();
        ApplicationServices.TryAddSingleton<GraphQLApplication>();
    }
}