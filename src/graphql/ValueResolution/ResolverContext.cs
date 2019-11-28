using System;
using System.Collections.Generic;
using GraphQLParser.AST;
using Tanka.GraphQL.Execution;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.ValueResolution
{
    public class ResolverContext : IResolverContext
    {
        public ResolverContext(
            ObjectType objectType,
            object objectValue,
            IField field,
            GraphQLFieldSelection selection,
            IReadOnlyCollection<GraphQLFieldSelection> fields,
            IReadOnlyDictionary<string, object> arguments,
            NodePath path,
            IExecutorContext executionContext)
        {
            ObjectType = objectType ?? throw new ArgumentNullException(nameof(objectType));
            ObjectValue = objectValue;
            Field = field ?? throw new ArgumentNullException(nameof(field));
            Selection = selection ?? throw new ArgumentNullException(nameof(selection));
            Fields = fields;
            Arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
            Path = path ?? throw new ArgumentNullException(nameof(path));
            ExecutionContext = executionContext;
        }

        public IDictionary<object, object> Items { get; } = new Dictionary<object, object>();

        public ISchema Schema => ExecutionContext.Schema;

        public ObjectType ObjectType { get; }

        public object ObjectValue { get; }

        public IField Field { get; }

        public GraphQLFieldSelection Selection { get; }
        public IReadOnlyCollection<GraphQLFieldSelection> Fields { get; }

        public IReadOnlyDictionary<string, object> Arguments { get; }

        public NodePath Path { get; }

        public IExecutorContext ExecutionContext { get; }

        public string FieldName => Selection.Name?.Value;
    }
}