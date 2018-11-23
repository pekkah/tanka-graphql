using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;

namespace fugu.graphql
{
    public class SubscriptionResult : IExecutionResult
    {
        private IEnumerable<Error> _errors;

        public SubscriptionResult(ISourceBlock<ExecutionResult> source)
        {
            Source = source;
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
    }
}