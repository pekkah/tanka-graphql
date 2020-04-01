using System;
using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Execution
{
    public class ExecutorContext : IExecutorContext
    {
        private readonly List<Exception> _errors;

        public ExecutorContext(ISchema schema,
            ExecutableDocument document,
            ExtensionsRunner extensionsRunner,
            IExecutionStrategy strategy,
            OperationDefinition operation,
            IDictionary<string, FragmentDefinition> fragments, 
            IReadOnlyDictionary<string, object?> coercedVariableValues)
        {
            Schema = schema ?? throw new ArgumentNullException(nameof(schema));
            Document = document ?? throw new ArgumentNullException(nameof(document));
            ExtensionsRunner = extensionsRunner ?? throw new ArgumentNullException(nameof(extensionsRunner));
            Strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
            Operation = operation;
            Fragments = fragments;
            CoercedVariableValues = coercedVariableValues;
            _errors = new List<Exception>();
        }

        public OperationDefinition Operation { get; }

        public IDictionary<string, FragmentDefinition> Fragments { get; }

        public IReadOnlyDictionary<string, object?> CoercedVariableValues { get; }

        public ISchema Schema { get; }

        public ExecutableDocument Document { get; }

        public ExtensionsRunner ExtensionsRunner { get; }

        public IEnumerable<Exception> FieldErrors => _errors;

        public IExecutionStrategy Strategy { get; }

        public void AddError(Exception error)
        {
            if (_errors.Contains(error))
                return;

            _errors.Add(error);
        }
    }
}