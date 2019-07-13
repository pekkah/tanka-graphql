using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQLParser.AST;
using tanka.graphql.type;

namespace tanka.graphql.execution
{
    public class ParallelExecutionStrategy : ExecutionStrategyBase
    {
        public override async Task<IDictionary<string, object>> ExecuteGroupedFieldSetAsync(
            IExecutorContext context,
            IReadOnlyDictionary<string, List<GraphQLFieldSelection>> groupedFieldSet,
            ObjectType objectType,
            object objectValue,
            NodePath path)
        {
            var data = new ConcurrentDictionary<string, object>();
            var tasks = new ConcurrentBag<Task>();
            foreach (var fieldGroup in groupedFieldSet)
            {
                var executionTask = Task.Run(async () =>
                {
                    var responseKey = fieldGroup.Key;
                    var result = await ExecuteFieldGroupAsync(
                        context,
                        objectType,
                        objectValue,
                        //todo: following is dirty
                        new KeyValuePair<string, IReadOnlyCollection<GraphQLFieldSelection>>(fieldGroup.Key, fieldGroup.Value), 
                        path.Fork()).ConfigureAwait(false);

                    data[responseKey] = result;
                });

                tasks.Add(executionTask);
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
            return data;
        }
    }
}