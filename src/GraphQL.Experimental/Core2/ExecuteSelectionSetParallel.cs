using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Experimental.Core2
{
    public class ExecuteSelectionSetParallel
    {
        private readonly FieldCollector _fieldCollector;
        private readonly FieldExecutor _fieldExecutor;

        public ExecuteSelectionSetParallel(
            FieldCollector fieldCollector,
            FieldExecutor fieldExecutor)
        {
            _fieldCollector = fieldCollector;
            _fieldExecutor = fieldExecutor;
        }

        public async Task<IReadOnlyDictionary<string, object?>> ExecuteSelectionSet(
            OperationContext context,
            ObjectDefinition objectDefinition,
            object? objectValue,
            SelectionSet selectionSet,
            NodePath path,
            CancellationToken cancellationToken)
        {
            var groupedFieldSet = _fieldCollector.CollectFields(
                context,
                objectDefinition,
                selectionSet,
                cancellationToken: cancellationToken);

            var tasks = new Dictionary<string, Task<object?>>();

            foreach (var (responseKey, fields) in groupedFieldSet)
            {
                var fieldPath = path.Fork();
                var fieldName = fields.First().Name;
                var fieldType = context.Schema
                    .GetField(objectDefinition, fieldName)
                    ?.Type;

                fieldPath.Append(fieldName);

                if (fieldType != null)
                    tasks.Add(responseKey, _fieldExecutor.ExecuteField(
                        context,
                        objectDefinition,
                        objectValue,
                        fieldType,
                        fields,
                        fieldPath,
                        cancellationToken));
            }

            await Task.WhenAll(tasks.Values);

            var data = tasks
                .ToDictionary(
                    kv => kv.Key,
                    kv => kv.Value.Result
                );

            return data;
        }
    }
}