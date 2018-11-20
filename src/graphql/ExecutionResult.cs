using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace fugu.graphql
{
    public class ExecutionResult : IExecutionResult
    {
        private IDictionary<string, object> _data;
        private IDictionary<string, object> _extensions;
        private IEnumerable<Error> _errors;

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

        public void AddExtension(string key, object value)
        {
            if (Extensions == null)
            {
                Extensions = new Dictionary<string, object>()
                {
                    {key, value}
                };
                return;
            }

            Extensions[key] = value;
        }
    }
}