using System.Collections.Generic;
using Tanka.GraphQL.Execution;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.ValueResolution;

public interface IResolverContext
{
    IReadOnlyDictionary<string, object?> Arguments { get; }

    IExecutorContext ExecutionContext { get; }

    FieldDefinition Field { get; }

    string FieldName { get; }

    IReadOnlyCollection<FieldSelection> Fields { get; }

    IDictionary<object, object> Items { get; }
    ObjectDefinition ObjectDefinition { get; }

    object? ObjectValue { get; }

    NodePath Path { get; }

    ISchema Schema => ExecutionContext.Schema;

    FieldSelection Selection { get; }
}