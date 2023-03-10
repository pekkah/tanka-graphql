using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Features;

public interface IFieldExecutorFeature
{
    Task<object?> Execute(
        QueryContext context,
        ObjectDefinition objectDefinition,
        object? objectValue,
        IReadOnlyCollection<FieldSelection> fields,
        NodePath path);
}