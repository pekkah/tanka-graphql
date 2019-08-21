using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQLParser.AST;
using tanka.graphql.type;

namespace tanka.graphql.execution
{
    public class ParallelExecutionStrategy : IExecutionStrategy
    {
        public async Task<IDictionary<string, object>> ExecuteGroupedFieldSetAsync(
            IExecutorContext context,
            IReadOnlyDictionary<string, List<GraphQLFieldSelection>> groupedFieldSet,
            ObjectType objectType,
            object objectValue,
            NodePath path)
        {
            var tasks = new Dictionary<string, Task<object>>();
            foreach (var fieldGroup in groupedFieldSet)
            {
                var executionTask = FieldGroups.ExecuteFieldGroupAsync(
                    context,
                    objectType,
                    objectValue,
                    //todo: following is dirty
                    new KeyValuePair<string, IReadOnlyCollection<GraphQLFieldSelection>>(fieldGroup.Key,
                        fieldGroup.Value),
                    path.Fork());

                tasks.Add(fieldGroup.Key, executionTask);
            }

            await Task.WhenAll(tasks.Values).ConfigureAwait(false);
            return tasks.ToDictionary(kv => kv.Key, kv => kv.Value.Result);
        }
    }
}