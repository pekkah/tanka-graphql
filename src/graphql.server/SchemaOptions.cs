using Tanka.GraphQL.Executable;

namespace Tanka.GraphQL.Server;

public class SchemaOptions
{
    public string SchemaName { get; set; } = string.Empty;

    public ExecutableSchemaBuilder Builder { get; } = new();
}