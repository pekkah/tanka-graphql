using System;
using System.Collections.Concurrent;

namespace Tanka.GraphQL.Experimental
{
    public class RequestPipelineBuilder
    {
        private readonly ConcurrentQueue<CoerceVariableValues> _coerceVariableValues = new();
        private readonly ConcurrentQueue<OperationExecutorSelector> _executorSelectors = new();
        private readonly ConcurrentQueue<OperationSelector> _operationSelectors = new();
        private readonly ConcurrentQueue<ValidateOperation> _operationValidators = new();

        public RequestPipelineBuilder UseOperationSelector(OperationSelector selector)
        {
            _operationSelectors.Enqueue(selector);
            return this;
        }

        public RequestPipelineBuilder UseCoerceVariableValues(CoerceVariableValues coerceVariableValues)
        {
            _coerceVariableValues.Enqueue(coerceVariableValues);
            return this;
        }

        public RequestPipelineBuilder UseValidation(ValidateOperation validator)
        {
            _operationValidators.Enqueue(validator);
            return this;
        }

        public RequestPipelineBuilder UseOperationSelector(OperationExecutorSelector executorSelector)
        {
            _executorSelectors.Enqueue(executorSelector);
            return this;
        }

        public RequestPipeline Build()
        {
            return new(
                BuildOperationSelector(),
                BuildVariableValueCoercer(),
                BuildOperationValidator(),
                BuildOperationExecutorSelector()
            );
        }

        private OperationExecutorSelector BuildOperationExecutorSelector()
        {
            if (!_executorSelectors.TryDequeue(out var selector))
                throw new InvalidOperationException($"{nameof(OperationExecutorSelector)} is required");

            while (_executorSelectors.TryDequeue(out var next)) selector += next;

            return selector;
        }

        private ValidateOperation BuildOperationValidator()
        {
            if (!_operationValidators.TryDequeue(out var validator))
                throw new InvalidOperationException($"{nameof(ValidateOperation)} is required");

            while (_operationValidators.TryDequeue(out var next)) validator += next;

            return validator;
        }

        private CoerceVariableValues BuildVariableValueCoercer()
        {
            if (!_coerceVariableValues.TryDequeue(out var coercer))
                throw new InvalidOperationException($"{nameof(CoerceVariableValues)} is required");

            while (_coerceVariableValues.TryDequeue(out var next)) coercer += next;

            return coercer;
        }

        private OperationSelector BuildOperationSelector()
        {
            if (!_operationSelectors.TryDequeue(out var selector))
                throw new InvalidOperationException($"{nameof(OperationSelector)} is required");

            while (_operationSelectors.TryDequeue(out var next)) selector += next;

            return selector;
        }
    }
}