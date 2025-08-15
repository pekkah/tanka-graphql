using Tanka.GraphQL.Executable;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Server;

public class SchemaOptions
{
    public ExecutableSchemaBuilder Builder { get; } = new();

    private readonly List<Action<SchemaBuildOptions>> _configureBuildActions = new();

    public void ConfigureBuild(Action<SchemaBuildOptions> configure)
    {
        _configureBuildActions.Add(configure);
    }

    internal void ApplyBuildConfiguration(SchemaBuildOptions options)
    {
        foreach (Action<SchemaBuildOptions> configureBuildAction in _configureBuildActions)
        {
            configureBuildAction(options);
        }
    }
}