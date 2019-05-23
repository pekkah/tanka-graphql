﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using tanka.graphql.error;
using tanka.graphql.type;
using GraphQLParser.AST;

namespace tanka.graphql.execution
{
    public class SerialExecutionStrategy : ExecutionStrategyBase
    {
        public override async Task<IDictionary<string, object>> ExecuteGroupedFieldSetAsync(IExecutorContext context,
            Dictionary<string, List<GraphQLFieldSelection>> groupedFieldSet,
            ObjectType objectType, object objectValue,
            IReadOnlyDictionary<string, object> coercedVariableValues,
            NodePath path)
        {
            var responseMap = new Dictionary<string, object>();

            foreach (var fieldGroup in groupedFieldSet)
            {
                var responseKey = fieldGroup.Key;

                try
                {
                    var result = await ExecuteFieldGroupAsync(
                        context,
                        objectType,
                        objectValue,
                        coercedVariableValues,
                        fieldGroup,
                        path.Fork()).ConfigureAwait(false);

                    responseMap[responseKey] = result;
                }
                catch (GraphQLError e)
                {
                    responseMap[responseKey] = null;
                    context.AddError(e);
                }
            }

            return responseMap;
        }
    }
}