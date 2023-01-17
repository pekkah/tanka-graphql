namespace Tanka.GraphQL;

public partial class Executor
{
    public QueryContext BuildQueryContextAsync(GraphQLRequest request)
    {
        var queryContext = new QueryContext(_defaults);

        var document = request.Document;
        var operation = Operations.GetOperation(document, request.OperationName);

        var coercedVariableValues = Variables.CoerceVariableValues(
            queryContext.Schema,
            operation,
            request.VariableValues);

        queryContext.CoercedVariableValues = coercedVariableValues;
        queryContext.OperationDefinition = operation;
        queryContext.Request = request;

        return queryContext;
    }
}