using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Server.Links
{
    public class PreExecutedResolverResult : IResolverResult
    {
        private readonly IDictionary<string, object> _data;
        private readonly IEnumerable<ExecutionError> _errors;
        private readonly Dictionary<string, object> _extensions;

        public PreExecutedResolverResult(ExecutionResult executionResult)
        {
            _data = executionResult.Data;
            _errors = executionResult.Errors;
            _extensions = executionResult.Extensions;
        }

        public object Value => _data;

        public ValueTask<object> CompleteValueAsync(IResolverContext context)
        {
            if (_errors.Any())
            {
                var first = _errors.First();
                throw new CompleteValueException(
                    $"{first.Message}",
                    null,
                    context.Path,
                    new Dictionary<string, object>
                    {
                        ["remoteError"] = new
                        {
                            error = first,
                            data = _data,
                            errors = _errors,
                            extensions = _extensions
                        }
                    },
                    context.Selection);
            }

            var value = _data[context.FieldName];
            var resolveResult = new CompleteValueResult(value, context.Field.Type);
            return resolveResult.CompleteValueAsync(context);
        }
    }
}