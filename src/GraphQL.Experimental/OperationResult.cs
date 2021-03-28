using System.Collections.Generic;
using System.Linq;

namespace Tanka.GraphQL.Experimental
{
    public record OperationResult
    {
        private IReadOnlyDictionary<string, object>? _data;
        private IReadOnlyList<FieldError>? _errors;
        private IReadOnlyDictionary<string, object>? _extensions;

        public IReadOnlyDictionary<string, object>? Data
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

        public IReadOnlyDictionary<string, object>? Extensions
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

        public IReadOnlyList<FieldError>? Errors
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