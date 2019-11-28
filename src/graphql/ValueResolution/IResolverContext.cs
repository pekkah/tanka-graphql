using System.Collections.Generic;
using GraphQLParser.AST;
using Tanka.GraphQL.Execution;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.ValueResolution
{
    public interface IResolverContext
    {
        ObjectType ObjectType { get; }

        object ObjectValue { get; }

        IField Field { get; }

        GraphQLFieldSelection Selection { get; }

        IReadOnlyDictionary<string, object> Arguments { get; }

        NodePath Path { get; }

        IExecutorContext ExecutionContext { get; }

        string FieldName { get; }

        IDictionary<object, object> Items { get; }

        IReadOnlyCollection<GraphQLFieldSelection> Fields { get; }
    }
}