using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace fugu.graphql
{
    public class SubscriptionResult : IExecutionResult
    {
        private readonly Func<Task> _unsubscribe;
        private IEnumerable<Error> _errors;

        public SubscriptionResult(ISourceBlock<ExecutionResult> source, Func<Task> unsubscribe)
        {
            Source = source;
            _unsubscribe = unsubscribe;
        }

        public SubscriptionResult()
        {
        }

        public ISourceBlock<ExecutionResult> Source { get; }

        public IEnumerable<Error> Errors
        {
            get => _errors;
            set
            {
                if (value != null)
                    if (!value.Any())
                    {
                        _errors = null;
                        return;
                    }

                _errors = value;
            }
        }

        public Task UnsubscribeAsync()
        {
            return _unsubscribe();
        }
    }
}