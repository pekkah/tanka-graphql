using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Tanka.GraphQL.Server;

internal class SchemaInitializer : IHostedService
{
    private readonly IOptionsMonitor<GraphQLApplicationOptions> _applicationOptions;
    private readonly IOptionsMonitor<SchemaOptions> _schemaOptions;
    private readonly SchemaCollection _schemas;

    public SchemaInitializer(
        SchemaCollection schemas,
        IOptionsMonitor<SchemaOptions> schemaOptions,
        IOptionsMonitor<GraphQLApplicationOptions> applicationOptions)
    {
        _schemas = schemas;
        _schemaOptions = schemaOptions;
        _applicationOptions = applicationOptions;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var appOptions = _applicationOptions.CurrentValue;

        foreach (var schemaName in appOptions.SchemaNames)
        {
            var options = _schemaOptions.Get(schemaName);

            var schema = await options.Builder.Build(buildOptions =>
            {
                options.ApplyBuildConfiguration(buildOptions);
            });

            _schemas.TryAdd(schemaName, schema);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
