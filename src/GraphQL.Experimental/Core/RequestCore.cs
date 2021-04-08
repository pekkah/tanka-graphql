using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.Experimental.Definitions;

namespace Tanka.GraphQL.Experimental.Core
{
    public class RequestCore
    {
        public static async IAsyncEnumerable<OperationResult> Execute(
            RequestOptions options,
            CreateOperationContext createOperationContext,
            object? initialValue = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var operationContext = await createOperationContext(
                options,
                initialValue,
                cancellationToken);

            if (operationContext.ValidationResult.HasErrors)
            {
                yield return new OperationResult
                {
                    Errors = operationContext?
                        .ValidationResult.Errors
                        .Select(ex => new FieldError(ex.Message, ex.Path, ex.Locations))
                        .ToList()
                };
            }
            else
            {
                var executor = operationContext.OperationExecutor;

                //todo: should be allow post processing of each result?
                await foreach (var result in executor(operationContext, options, initialValue, cancellationToken)
                    .WithCancellation(cancellationToken))
                    yield return result;
            }
        }

        public static async Task<OperationContext> CreateOperationContext(RequestOptions options,
            OperationSelector operationSelector,
            CoerceVariableValues coerceVariableValues,
            ValidateOperation validateOperation,
            OperationExecutorSelector executorSelector,
            ExecuteSelectionSetSelector executeSelectionSetSelector,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var plan = new OperationPlanContext();

            // select operation
            await operationSelector(plan, options, cancellationToken);

            if (plan.Operation == null)
                throw new InvalidOperationException("Operation is required");

            // coerce variables
            await coerceVariableValues(
                options.Schema,
                plan.Operation, 
                options.VariableValues,
                cancellationToken);

            // validate operation
            await validateOperation(plan, options, cancellationToken);

            // select operation executor
            await executorSelector(plan, options, cancellationToken);

            // select selection set executor
            await executeSelectionSetSelector(plan, options, cancellationToken);

            return new OperationContext(
                options.Schema,
                options.Document,
                plan.Operation,
                plan.CoercedVariableValues ?? new Dictionary<string, object?>(),
                plan.ValidationResult ?? OperationValidationResult.Success,
                plan.OperationExecutor ?? throw new InvalidOperationException("OperationExecutor is required"),
                plan.ExecuteSelectionSet ?? throw new InvalidOperationException("ExecuteSelectionSet is required")
            );
        }
    }
}