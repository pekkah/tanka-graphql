using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Tanka.GraphQL.Executable;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Server;

public class SchemaOptionsBuilder
{
    public SchemaOptionsBuilder(
        OptionsBuilder<SchemaOptions> builder,
        IServiceCollection services)
    {
        Services = services;
        Builder = builder;
        Builder.Configure(options => options.SchemaName = builder.Name);
    }

    public IServiceCollection Services { get; }

    public OptionsBuilder<SchemaOptions> Builder { get; }

    public SchemaOptionsBuilder Configure(Action<ExecutableSchemaBuilder> configureAction)
    {
        Builder.Configure(options => configureAction(options.Builder));
        return this;
    }

    public SchemaOptionsBuilder Configure<T>(Action<ExecutableSchemaBuilder, T> configureAction) where T : class
    {
        Builder.Configure<T>((options, dep1) => configureAction(options.Builder, dep1));
        return this;
    }

    public SchemaOptionsBuilder PostConfigure<T>(Action<ExecutableSchemaBuilder, T> configureAction) where T : class
    {
        Builder.PostConfigure<T>((options, dep1) => configureAction(options.Builder, dep1));
        return this;
    }

    public SchemaOptionsBuilder AddConfiguration(IExecutableSchemaConfiguration configuration)
    {
        Builder.Configure(options => options.Builder.AddConfiguration(configuration));
        return this;
    }

    public SchemaOptionsBuilder AddTypeSystem(TypeSystemDocument document)
    {
        Builder.Configure(options => options.Builder.AddTypeSystem(document));
        return this;
    }
}