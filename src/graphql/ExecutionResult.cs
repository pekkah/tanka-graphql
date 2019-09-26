using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Tanka.GraphQL.DTOs;

namespace Tanka.GraphQL
{
    /// <summary>
    ///     Result of query, mutation or value of one value in the
    ///     <see cref="SubscriptionResult" /> stream
    /// </summary>
    public class ExecutionResult : IExecutionResult
    {
        private IDictionary<string, object> _data;
        private IEnumerable<ExecutionError> _errors;
        private IDictionary<string, object> _extensions;

        [JsonConverter(typeof(NestedDictionaryConverter))]
        [DataMember(Name = "data")]
        public IDictionary<string, object> Data
        {
            get => _data;
            set
            {
                if (value != null && !value.Any())
                {
                    _data = null;
                    return;
                }

                _data = value;
            }
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [DataMember(Name = "extensions")]
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
        [DataMember(Name = "errors")]
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