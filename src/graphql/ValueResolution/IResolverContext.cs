using System.Collections.Generic;

using Tanka.GraphQL.Execution;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.ValueResolution
{
    public interface IResolverContext
    {
        ObjectDefinition ObjectDefinition { get; }

        object? ObjectValue { get; }

        FieldDefinition Field { get; }

        FieldSelection Selection { get; }

        IReadOnlyDictionary<string, object?> Arguments { get; }

        NodePath Path { get; }

        IExecutorContext ExecutionContext { get; }

        string FieldName { get; }

        IDictionary<object, object> Items { get; }

        IReadOnlyCollection<FieldSelection> Fields { get; }

        ISchema Schema => ExecutionContext.Schema;
    }
}