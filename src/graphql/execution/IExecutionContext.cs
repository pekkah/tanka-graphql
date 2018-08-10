using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using fugu.graphql.type;
using GraphQLParser.AST;

namespace fugu.graphql.execution
{
    public interface IExecutionContext
    {
        ISchema Schema { get; }

        GraphQLDocument Document { get; }

        List<Exception> FieldErrors { get; }

        Task<IDictionary<string, object>> ExecuteGroupedFieldSetAsync(
            Dictionary<string, List<GraphQLFieldSelection>> groupedFieldSet,
            ObjectType objectType, object objectValue,
            Dictionary<string, object> coercedVariableValues);
    }
}