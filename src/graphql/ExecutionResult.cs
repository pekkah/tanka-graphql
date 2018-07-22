using System.Collections.Generic;
using System.Linq;

namespace fugu.graphql
{
    public class ExecutionResult : IExecutionResult
    {
        private IEnumerable<Error> _errors;
        private IDictionary<string, object> _data;

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
    }
}