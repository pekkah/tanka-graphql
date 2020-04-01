using System.Collections.Generic;

using Tanka.GraphQL.Execution;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.ValueResolution
{
    public interface IResolverContext
    {
        ObjectType ObjectType { get; }

        object ObjectValue { get; }

        IField Field { get; }

        FieldSelection Selection { get; }

        IReadOnlyDictionary<string, object> Arguments { get; }

        NodePath Path { get; }

        IExecutorContext ExecutionContext { get; }

        string FieldName { get; }

        IDictionary<object, object> Items { get; }

        IReadOnlyCollection<FieldSelection> Fields { get; }
    }
}