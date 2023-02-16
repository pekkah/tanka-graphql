using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Validation;

namespace Tanka.GraphQL;

public class DefaultOperationExecutorFeature : IOperationExecutorFeature
{
    public async Task Execute(QueryContext context)
    {
        ValidationResult validationResult = await context.Validate();

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult);

        Task execute = context.OperationDefinition.Operation switch
        {
            OperationType.Query => Executor.ExecuteQueryOrMutation(context),
            OperationType.Mutation => Executor.ExecuteQueryOrMutation(context),
            OperationType.Subscription => Executor.ExecuteSubscription(context),
            _ => throw new InvalidOperationException(
                $"Operation type {context.OperationDefinition.Operation} not supported.")
        };

        await execute;
    }
}