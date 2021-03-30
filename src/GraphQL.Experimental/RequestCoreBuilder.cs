namespace Tanka.GraphQL.Experimental
{
    public class RequestCoreBuilder
    {
        public static ExecuteRequestSingle BuildExecuteSingle(
            ExecuteRequest executeRequest)
        {
            return async (options, initialValue, cancellationToken) =>
            {
                var stream = executeRequest(options, initialValue, cancellationToken);
                await using var iterator = stream.GetAsyncEnumerator(cancellationToken);

                // get first item
                await iterator.MoveNextAsync();
                return iterator.Current;
            };
        }

        public static ExecuteRequest BuildExecute(
            CreateOperationContext createOperationContext)
        {
            return (options, initialValue, cancellationToken) =>
                RequestCore.Execute(
                    options,
                    createOperationContext,
                    initialValue,
                    cancellationToken);
        }

        public static CreateOperationContext BuildCreateOperationContext(
            OperationSelector operationSelector,
            CoerceVariableValues coerceVariableValues,
            ValidateOperation validateOperation,
            OperationExecutorSelector executorSelector,
            ExecuteSelectionSetSelector executeSelectionSetSelector,
            CoerceValue coerceValue)
        {
            //todo: should pass initial value or not?
            return (options, _, cancellationToken) =>
                RequestCore.CreateOperationContext(
                    options,
                    operationSelector,
                    coerceVariableValues,
                    validateOperation,
                    executorSelector,
                    executeSelectionSetSelector,
                    cancellationToken);
        }
    }
}