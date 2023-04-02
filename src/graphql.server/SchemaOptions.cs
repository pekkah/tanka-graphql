using Tanka.GraphQL.Executable;

namespace Tanka.GraphQL.Server;

public class SchemaOptions
{
    public ExecutableSchemaBuilder Builder { get; } = new();
}