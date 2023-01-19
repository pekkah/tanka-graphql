using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Tanka.GraphQL.Server;

public class SchemaOptionsBuilder
{
    public SchemaOptionsBuilder(
        OptionsBuilder<SchemaOptions> builder,
        IServiceCollection applicationServices)
    {
        ApplicationServices = applicationServices;
        Builder = builder;
        Builder.Configure(options => options.SchemaName = builder.Name);
    }

    public IServiceCollection ApplicationServices { get; }

    public OptionsBuilder<SchemaOptions> Builder { get; }

    public SchemaOptionsBuilder Configure(Action<ExecutableSchemaBuilder> configureAction)
    {
        Builder.Configure(options => configureAction(options.Builder));
        return this;
    }

    public SchemaOptionsBuilder AddConfiguration(IExecutableSchemaConfiguration configuration)
    {
        Builder.Configure(options => options.Builder.AddConfiguration(configuration));
        return this;
    }
}