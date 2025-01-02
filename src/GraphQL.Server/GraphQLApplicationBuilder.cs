using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Tanka.GraphQL.Executable;

namespace Tanka.GraphQL.Server;

public class GraphQLApplicationBuilder
{
    public GraphQLApplicationBuilder(IServiceCollection applicationServices)
    {
        ApplicationServices = applicationServices;
        ApplicationOptionsBuilder = ApplicationServices
            .AddOptions<GraphQLApplicationOptions>();

        AddDefaultTankaGraphQLServerServices();
    }

    public IServiceCollection ApplicationServices { get; }

    public OptionsBuilder<GraphQLApplicationOptions> ApplicationOptionsBuilder { get; }

    public GraphQLApplicationBuilder AddHttp()
    {
        ApplicationServices.TryAddSingleton<GraphQLHttpTransportMiddleware>();
        ApplicationServices.TryAddEnumerable(ServiceDescriptor.Singleton<IGraphQLTransport, GraphQLHttpTransport>());
        return this;
    }

    public GraphQLApplicationBuilder AddSchemaOptions(
        string schemaName,
        Action<OptionsBuilder<SchemaOptions>> configureOptions)
    {
        OptionsBuilder<SchemaOptions> schemaOptions = ApplicationServices
            .AddOptions<SchemaOptions>(schemaName);
        
        configureOptions(schemaOptions);

        ApplicationOptionsBuilder.Configure(options => options.SchemaNames.Add(schemaName));

        return this;
    }

    public GraphQLApplicationBuilder AddSchema(
        string schemaName,
        Action<ExecutableSchemaBuilder> configureExecutable)
    {
        return AddSchemaOptions(schemaName, builder => builder.Configure(opt => configureExecutable(opt.Builder)));
    }

    public GraphQLApplicationBuilder AddWebSockets()
    {
        ApplicationServices.TryAddEnumerable(ServiceDescriptor.Singleton<IGraphQLTransport, GraphQLWSTransport>());
        return this;
    }

    private void AddDefaultTankaGraphQLServerServices()
    {
        ApplicationServices.AddHostedService<SchemaInitializer>();
        ApplicationServices.TryAddSingleton<SchemaCollection>();
        ApplicationServices.TryAddSingleton<GraphQLApplication>();
        ApplicationServices.AddDefaultTankaGraphQLServices();
    }
}