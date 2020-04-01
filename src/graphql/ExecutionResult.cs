using System.Collections.Generic;
using System.Linq;

namespace Tanka.GraphQL
{
    /// <summary>
    ///     Result of query, mutation or value of one value in the
    ///     <see cref="SubscriptionResult" /> stream
    /// </summary>
    public class ExecutionResult : IExecutionResult
    {
        private Dictionary<string, object>? _data;
        private List<ExecutionError>? _errors;
        private Dictionary<string, object>? _extensions;

        public Dictionary<string, object>? Data
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

        public Dictionary<string, object>? Extensions
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

        public List<ExecutionError>? Errors
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