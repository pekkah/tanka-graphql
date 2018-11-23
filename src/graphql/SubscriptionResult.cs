using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;

namespace fugu.graphql
{
    /// <summary>
    ///     Result of executing a subscription
    /// </summary>
    public class SubscriptionResult : IExecutionResult
    {
        private IEnumerable<Error> _errors;

        public SubscriptionResult(ChannelReader<ExecutionResult> reader)
        {
            Reader = reader;
        }

        public SubscriptionResult()
        {
        }

        public ChannelReader<ExecutionResult> Reader { get; }

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