using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using fugu.graphql.type;
using GraphQLParser.AST;

namespace fugu.graphql.execution
{
    public class SerialExecutionContext : ExecutionContextBase
    {
        public SerialExecutionContext(ISchema schema, GraphQLDocument document) 
            : base(schema, document)
        {
        }

        public override async Task<IDictionary<string, object>> ExecuteGroupedFieldSetAsync(Dictionary<string, List<GraphQLFieldSelection>> groupedFieldSet, ObjectType objectType, object objectValue,
            Dictionary<string, object> coercedVariableValues)
        {
            var responseMap = new Dictionary<string, object>();

            foreach (var fieldGroup in groupedFieldSet)
            {
                var responseKey = fieldGroup.Key;

                try
                {
                    var result = await ExecuteFieldGroupAsync(
                        objectType,
                        objectValue,
                        coercedVariableValues,
                        fieldGroup).ConfigureAwait(false);

                    responseMap[responseKey] = result;
                }
                catch (Exception e)
                {
                    responseMap[responseKey] = null;
                    FieldErrors.Add(e);
                }
            }

            return responseMap;
        }
    }
}