using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using tanka.graphql.type;
using GraphQLParser.AST;

namespace tanka.graphql.execution
{
    public class SerialExecutionStrategy : IExecutionStrategy
    {
        public async Task<IDictionary<string, object>> ExecuteGroupedFieldSetAsync(
            IExecutorContext context,
            IReadOnlyDictionary<string, List<GraphQLFieldSelection>> groupedFieldSet,
            ObjectType objectType, object objectValue,
            NodePath path)
        {
            var responseMap = new Dictionary<string, object>();

            foreach (var fieldGroup in groupedFieldSet)
            {
                var responseKey = fieldGroup.Key;

                try
                {
                    var result = await FieldGroups.ExecuteFieldGroupAsync(
                        context,
                        objectType,
                        objectValue,
                        new KeyValuePair<string, IReadOnlyCollection<GraphQLFieldSelection>>(fieldGroup.Key, fieldGroup.Value), 
                        path.Fork()).ConfigureAwait(false);

                    responseMap[responseKey] = result;
                }
                catch (QueryExecutionException e)
                {
                    responseMap[responseKey] = null;
                    context.AddError(e);
                }
            }

            return responseMap;
        }
    }
}