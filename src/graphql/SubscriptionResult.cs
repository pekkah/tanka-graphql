using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using Newtonsoft.Json;

namespace Tanka.GraphQL
{
    /// <summary>
    ///     Result of executing a subscription
    /// </summary>
    public class SubscriptionResult : IExecutionResult
    {
        private IEnumerable<ExecutionError> _errors;
        private IDictionary<string, object> _extensions;

        public SubscriptionResult(Channel<ExecutionResult> source)
        {
            Source = source;
        }

        public SubscriptionResult()
        {
        }

        public Channel<ExecutionResult> Source { get; }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, object> Extensions
        {
            get => _extensions;
            set
            {
                if (value != null && !value.Any())
                {
                    _extensions = null;
                    return;
                }

                _extensions = value;
            }
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<ExecutionError> Errors
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