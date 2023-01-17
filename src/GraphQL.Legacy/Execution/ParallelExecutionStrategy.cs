using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Execution;

public class ParallelExecutionStrategy : SelectionSetExecutionStrategyBase
{
    protected override async Task<IDictionary<string, object?>> ExecuteGroupedFieldSetAsync(
        IExecutorContext context,
        IReadOnlyDictionary<string, List<FieldSelection>> groupedFieldSet,
        ObjectDefinition objectDefinition,
        object? objectValue,
        NodePath path)
    {
        var tasks = new Dictionary<string, Task<object?>>();
        foreach (var fieldGroup in groupedFieldSet)
        {
            var executionTask = FieldGroups.ExecuteFieldGroupAsync(
                context,
                objectDefinition,
                objectValue,
                //todo: following is dirty
                new KeyValuePair<string, IReadOnlyCollection<FieldSelection>>(fieldGroup.Key,
                    fieldGroup.Value),
                path.Fork());

            tasks.Add(fieldGroup.Key, executionTask);
        }

        await Task.WhenAll(tasks.Values).ConfigureAwait(false);
        return tasks.ToDictionary(kv => kv.Key, kv => kv.Value.Result);
    }
}