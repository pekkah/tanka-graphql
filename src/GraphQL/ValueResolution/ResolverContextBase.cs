using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.ValueResolution;

public class ResolverContextBase
{
    public required ObjectDefinition ObjectDefinition { get; set; }

    public required object? ObjectValue { get; set; }

    public required FieldDefinition? Field { get; set; }

    public required FieldSelection Selection { get; set; }

    public required IReadOnlyCollection<FieldSelection> Fields { get; set; }

    public required IReadOnlyDictionary<string, object?> ArgumentValues { get; set; }

    public required NodePath Path { get; set; }

    public required QueryContext QueryContext { get; set; }

    public ISchema Schema => QueryContext.Schema;

    public string FieldName => Selection.Name.Value;
}