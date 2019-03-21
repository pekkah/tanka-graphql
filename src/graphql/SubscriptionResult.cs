using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;

namespace tanka.graphql
{
    /// <summary>
    ///     Result of executing a subscription
    /// </summary>
    public class SubscriptionResult : IExecutionResult
    {
        private IEnumerable<Error> _errors;

        public SubscriptionResult(ChannelReader<ExecutionResult> source)
        {
            Source = source;
        }

        public SubscriptionResult()
        {
        }

        public ChannelReader<ExecutionResult> Source { get; }


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