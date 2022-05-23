using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Tanka.GraphQL.Server;

public class GraphQLServiceBuilder
{
    public GraphQLServiceBuilder(IServiceCollection applicationServices)
    {
        ApplicationServices = applicationServices;
        ApplicationOptionsBuilder = ApplicationServices
            .AddOptions<GraphQLApplicationOptions>();

        AddCore();
    }

    public IServiceCollection ApplicationServices { get; }

    private GraphQLServiceBuilder AddCore()
    {
        
        ApplicationServices.TryAddSingleton<IHostedService, SchemaInitializer>();
        ApplicationServices.TryAddSingleton<SchemaCollection>();
        ApplicationServices.TryAddSingleton<GraphQLApplication>();

        return this;
    }

    public OptionsBuilder<GraphQLApplicationOptions> ApplicationOptionsBuilder { get; }

    public GraphQLServiceBuilder AddHttp()
    {
        ApplicationServices.TryAddSingleton<IGraphQLTransport, GraphQLHttpTransport>();
        return this;
    }

    public GraphQLServiceBuilder AddSchema(
        string schemaName,
        Action<SchemaOptionsBuilder> configureOptions)
    {
        var schemaOptions = ApplicationServices
            .AddOptions<SchemaOptions>(schemaName);
        
        var optionsBuilder = new SchemaOptionsBuilder(
            schemaOptions,
            ApplicationServices);

        configureOptions(optionsBuilder);

        ApplicationOptionsBuilder.Configure(options => options.SchemaNames.Add(schemaName));

        return this;
    }
}