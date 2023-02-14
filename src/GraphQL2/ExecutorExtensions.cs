using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL;

public static class ExecutorExtensions
{
    public static Task<ExecutionResult> Execute(
        this Executor executor,
        ExecutableDocument document,
        Dictionary<string, object?>? variableValues = null,
        object? initialValue = null,
        string? operationName = null,
        CancellationToken cancellationToken = default)
    {
        return executor.Execute(new GraphQLRequest()
        {
            Document = document,
            InitialValue = initialValue,
            OperationName = operationName,
            Variables = variableValues
        }, cancellationToken);
    }
}