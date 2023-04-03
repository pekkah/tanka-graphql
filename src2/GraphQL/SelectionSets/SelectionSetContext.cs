using System.Collections.Immutable;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.SelectionSets;

public record SelectionSetContext
{
    public required QueryContext QueryContext { get; set; }

    public required SelectionSet SelectionSet { get; set; }

    public required ObjectDefinition ObjectDefinition { get; set; }

    public required object? ObjectValue { get; set; }

    public required NodePath Path { get; set; }

    public IReadOnlyDictionary<string, object?> Result { get; set; } = ImmutableDictionary<string, object?>.Empty;

    public IReadOnlyDictionary<string, List<FieldSelection>> GroupedFieldSet { get; set; } = ImmutableDictionary<string, List<FieldSelection>>.Empty;
}