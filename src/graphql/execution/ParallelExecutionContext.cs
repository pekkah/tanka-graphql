using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using fugu.graphql.type;
using GraphQLParser.AST;

namespace fugu.graphql.execution
{
    public class ParallelExecutionContext : ExecutionContextBase
    {
        public ParallelExecutionContext(ISchema schema, GraphQLDocument document)
            : base(schema, document)
        {
        }

        public override async Task<IDictionary<string, object>> ExecuteGroupedFieldSetAsync(
            Dictionary<string, List<GraphQLFieldSelection>> groupedFieldSet,
            ObjectType objectType, object objectValue,
            Dictionary<string, object> coercedVariableValues)
        {
            var data = new ConcurrentDictionary<string, object>();
            var tasks = new ConcurrentBag<Task>();
            foreach (var fieldGroup in groupedFieldSet)
            {
                var executionTask = Task.Run(async () =>
                {
                    var responseKey = fieldGroup.Key;
                    var result = await ExecuteFieldGroupAsync(
                        objectType,
                        objectValue,
                        coercedVariableValues,
                        fieldGroup).ConfigureAwait(false);

                    data[responseKey] = result;
                });

                tasks.Add(executionTask);
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
            return data;
        }
    }
}