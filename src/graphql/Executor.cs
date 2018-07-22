using System;
using System.Linq;
using System.Threading.Tasks;
using fugu.graphql.execution;
using fugu.graphql.type;
using fugu.graphql.validation;
using GraphQLParser.AST;

namespace fugu.graphql
{
    public static class Executor
    {
        public static async Task<ExecutionResult> ExecuteAsync(ExecutionOptions options)
        {
            if (!options.Schema.IsInitialized)
                await options.Schema.InitializeAsync().ConfigureAwait(false);

            var operation = Operations.GetOperation(options.Document, options.OperationName);

            var coercedVariableValues = Variables.CoerceVariableValues(
                options.Schema,
                operation,
                options.VariableValues);

            if (options.Validate)
            {
                var result = await Validator.ValidateAsync(
                    options.Schema,
                    options.Document,
                    coercedVariableValues).ConfigureAwait(false);

                if (!result.IsValid)
                {
                    return new ExecutionResult()
                    {
                        Data = null,
                        Errors = result.Errors.Select(e => new Error(e.Message)).ToList()
                    };
                }
            }

            switch (operation.Operation)
            {
                case OperationType.Query:
                    return await Query.ExecuteQueryAsync(
                        options.ErrorTransformer,
                        options.Document,
                        operation,
                        options.Schema,
                        coercedVariableValues,
                        options.InitialValue).ConfigureAwait(false);
                case OperationType.Mutation:
                    return await Mutation.ExecuteMutationAsync(
                        options.ErrorTransformer,
                        options.Document,
                        operation,
                        options.Schema,
                        coercedVariableValues,
                        options.InitialValue).ConfigureAwait(false);
                case OperationType.Subscription:
                    throw new InvalidOperationException($"Use {nameof(SubscribeAsync)}");
                default:
                    throw new InvalidOperationException($"Operation type {operation.Operation} not supported.");
            }
        }

        public static async Task<SubscriptionResult> SubscribeAsync(ExecutionOptions options)
        {
            if (!options.Schema.IsInitialized) throw new InvalidOperationException();

            var operation = Operations.GetOperation(options.Document, options.OperationName);

            var coercedVariableValues = Variables.CoerceVariableValues(
                options.Schema,
                operation,
                options.VariableValues);

            if (options.Validate)
            {
                var result = await Validator.ValidateAsync(
                    options.Schema,
                    options.Document,
                    coercedVariableValues).ConfigureAwait(false);

                if (!result.IsValid)
                {
                    return new SubscriptionResult()
                    {
                        Errors = result.Errors.Select(e => new Error(e.Message)).ToList()
                    };
                }
            }

            switch (operation.Operation)
            {
                case OperationType.Subscription:
                    return await Subscription.SubscribeAsync(
                        options.ErrorTransformer,
                        options.Document,
                        operation,
                        options.Schema,
                        coercedVariableValues,
                        options.InitialValue).ConfigureAwait(false);
                default:
                    throw new InvalidOperationException(
                        $"Operation type {operation.Operation} not supported. Did you mean to use {nameof(ExecuteAsync)}?");
            }
        }
    }
}