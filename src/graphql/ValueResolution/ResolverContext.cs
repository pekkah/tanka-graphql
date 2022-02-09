using System;
using System.Collections.Generic;

using Tanka.GraphQL.Execution;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.ValueResolution
{
    public class ResolverContext : IResolverContext
    {
        public ResolverContext(
            ObjectDefinition objectDefinition,
            object? objectValue,
            FieldDefinition field,
            FieldSelection selection,
            IReadOnlyCollection<FieldSelection> fields,
            IReadOnlyDictionary<string, object?> arguments,
            NodePath path,
            IExecutorContext executionContext)
        {
            ObjectDefinition = objectDefinition ?? throw new ArgumentNullException(nameof(objectDefinition));
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

        public ObjectDefinition ObjectDefinition { get; }

        public object? ObjectValue { get; }

        public FieldDefinition Field { get; }

        public FieldSelection Selection { get; }
        
        public IReadOnlyCollection<FieldSelection> Fields { get; }

        public IReadOnlyDictionary<string, object?> Arguments { get; }

        public NodePath Path { get; }

        public IExecutorContext ExecutionContext { get; }

        public string FieldName => Selection.Name;
    }
}