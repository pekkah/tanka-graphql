using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;

namespace fugu.graphql
{
    /// <summary>
    ///     Result of executing a subscription
    /// </summary>
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

        /// <summary>
        ///     Source stream of <see cref="ExecutionResult"/>
        /// </summary>
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