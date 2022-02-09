﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Execution
{
    public class SerialExecutionStrategy : IExecutionStrategy
    {
        public async Task<IDictionary<string, object?>> ExecuteGroupedFieldSetAsync(
            IExecutorContext context,
            IReadOnlyDictionary<string, List<FieldSelection>> groupedFieldSet,
            ObjectDefinition objectDefinition,
            object? objectValue,
            NodePath path)
        {
            var responseMap = new Dictionary<string, object?>();

            foreach (var fieldGroup in groupedFieldSet)
            {
                var responseKey = fieldGroup.Key;

                try
                {
                    var result = await FieldGroups.ExecuteFieldGroupAsync(
                        context,
                        objectDefinition,
                        objectValue,
                        new KeyValuePair<string, IReadOnlyCollection<FieldSelection>>(fieldGroup.Key, fieldGroup.Value),
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