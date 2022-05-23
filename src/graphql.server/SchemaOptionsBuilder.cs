using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.ValueResolution;

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

    public SchemaOptionsBuilder AddHttpLink(
        string url)
    {
        Builder.Configure(options => { options.HttpLinks.Add(url); });

        return this;
    }

    public SchemaOptionsBuilder AddTypeSystem(
        ITypeSystemConfiguration configuration)
    {
        Builder.Configure(options => { options.Configurations.Add(configuration); });

        return this;
    }

    public SchemaOptionsBuilder AddTypeSystem(
        TypeSystemDocument document)
    {
        Builder.Configure(options =>
        {
            options.Documents.Add(document);
        });

        return this;
    }

    public SchemaOptionsBuilder ConfigureSchema(Func<SchemaBuilder, Task> configure)
    {
        Builder.Configure(options => { options.ConfigureSchema.Add(configure); });

        return this;
    }

    public SchemaOptionsBuilder ConfigureResolvers(Func<ResolversBuilder, Task> configure)
    {
        Builder.Configure(options => { options.ConfigureResolvers.Add(configure); });

        return this;
    }

    public SchemaOptionsBuilder AddResolver(string objectName, string fieldName, Resolver resolver)
    {
        Builder.Configure(options =>
        {
            options.ConfigureResolvers.Add(r =>
            {
                r.Add(objectName, fieldName, b => b.Run(resolver));
                return Task.CompletedTask;
            });
        });

        return this;
    }

    public SchemaOptionsBuilder ConfigureSchema(Action<SchemaBuilder> configure)
    {
        Builder.Configure(options =>
        {
            options.ConfigureSchema.Add(s =>
            {
                configure(s);
                return Task.CompletedTask;
            });
        });

        return this;
    }

    public SchemaOptionsBuilder ConfigureResolvers(Action<ResolversBuilder> configure)
    {
        Builder.Configure(options =>
        {
            options.ConfigureResolvers.Add(r =>
            {
                configure(r);
                return Task.CompletedTask;
            });
        });

        return this;
    }
}