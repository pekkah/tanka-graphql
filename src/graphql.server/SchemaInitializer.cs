using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Server;

internal class SchemaInitializer: BackgroundService
{
    private readonly SchemaCollection _schemas;
    private readonly IOptionsMonitor<SchemaOptions> _schemaOptions;
    private readonly IOptionsMonitor<GraphQLApplicationOptions> _applicationOptions;

    public SchemaInitializer(
        SchemaCollection schemas,
        IOptionsMonitor<SchemaOptions> schemaOptions,
        IOptionsMonitor<GraphQLApplicationOptions> applicationOptions)
    {
        _schemas = schemas;
        _schemaOptions = schemaOptions;
        _applicationOptions = applicationOptions;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var appOptions = _applicationOptions.CurrentValue;

        foreach (var schemaName in appOptions.SchemaNames)
        {
            var options = _schemaOptions.Get(schemaName);

            var schemaBuilder = new SchemaBuilder();
            var resolversBuilder = new ResolversBuilder();

            foreach (var typeSystemDocument in options.Documents)
            {
                schemaBuilder.Add(typeSystemDocument);
            }

            // remote links require refactoring links
            // into the main library
            //todo: Move remote links to GraphQL lib

            foreach (var configuration in options.Configurations)
            {
                configuration.Configure(schemaBuilder);
                configuration.Configure(resolversBuilder);
            }

            foreach (var configure in options.ConfigureSchema)
            {
                await configure(schemaBuilder);
            }

            foreach (var configure in options.ConfigureResolvers)
            {
                await configure(resolversBuilder);
            }

            var schema = await schemaBuilder.Build(new SchemaBuildOptions()
            {
                Resolvers = resolversBuilder.BuildResolvers(),
                Subscribers = resolversBuilder.BuildSubscribers(),
                BuildTypesFromOrphanedExtensions = true,
            });

            _schemas.TryAdd(schemaName, schema);
        }
    }
}