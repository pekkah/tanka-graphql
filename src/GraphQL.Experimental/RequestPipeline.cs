using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Tanka.GraphQL.Experimental
{
    public class RequestPipeline
    {
        private readonly CoerceVariableValues _coerceVariableValues;
        private readonly OperationExecutorSelector _executorSelector;
        private readonly OperationSelector _operationSelector;
        private readonly ValidateOperation _validateOperation;

        public RequestPipeline(
            OperationSelector operationSelector,
            CoerceVariableValues coerceVariableValues,
            ValidateOperation validateOperation,
            OperationExecutorSelector executorSelector)
        {
            _operationSelector = operationSelector;
            _coerceVariableValues = coerceVariableValues;
            _validateOperation = validateOperation;
            _executorSelector = executorSelector;
        }

        public async Task<OperationResult> ExecuteSingle(
            RequestOptions options,
            CancellationToken cancellationToken = default)
        {
            var stream = Execute(options, cancellationToken);
            await using var iterator = stream.GetAsyncEnumerator(cancellationToken);

            // get first item
            await iterator.MoveNextAsync();
            return iterator.Current;
        }

        public async IAsyncEnumerable<OperationResult> Execute(
            RequestOptions options,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var operationContext = await BuildOperationContext(options, cancellationToken);

            if (operationContext.ValidationResult.HasErrors)
            {
                yield return new OperationResult
                {
                    Errors = operationContext?.ValidationResult.Errors.ToList()
                };

                yield break;
            }

            var executor = operationContext.OperationExecutor;

            //todo: should be allow post processing of each result?
            await foreach (var result in executor(operationContext, options, cancellationToken)
                .WithCancellation(cancellationToken))
                yield return result;
        }

        private async Task<OperationContext> BuildOperationContext(
            RequestOptions options,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var plan = new OperationPlanContext();

            // select operation
            await _operationSelector(plan, options, cancellationToken);

            // coerce variables
            await _coerceVariableValues(plan, options, cancellationToken);

            // validate operation
            await _validateOperation(plan, options, cancellationToken);

            // select operation executor
            await _executorSelector(plan, options, cancellationToken);

            return new OperationContext(
                plan.Operation ?? throw new InvalidOperationException("Operation is required"),
                plan.CoercedVariableValues ?? new Dictionary<string, object>(),
                plan.ValidationResult ?? OperationValidationResult.Success,
                plan.OperationExecutor ?? throw new InvalidOperationException("OperationExecutor is required")
            );
        }
    }
}