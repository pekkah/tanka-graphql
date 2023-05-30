using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Server;

public class SchemaCollection
{
    private readonly Dictionary<string, ISchema> _schemas = new();

    public IReadOnlyDictionary<string, ISchema> Schemas => _schemas;

    public ISchema Get(string name)
    {
        if (_schemas.TryGetValue(name, out ISchema? schema))
            return schema;

        throw new InvalidOperationException(
            $"Schema '{name}' not known"
        );
    }

    public bool TryAdd(string name, ISchema schema)
    {
        return _schemas.TryAdd(name, schema);
    }
}