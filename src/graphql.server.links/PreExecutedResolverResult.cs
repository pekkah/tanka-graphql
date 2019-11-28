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
            if (_errors != null && _errors.Any())
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

            if (!_data.TryGetValue(context.FieldName, out var value))
                throw new CompleteValueException(
                    $"Could not complete value for field '{context.FieldName}:{context.Field.Type}'. " +
                    $"Could not find field value from execution result. Fields found '{string.Join(",", _data.Keys)}'",
                    context.Path,
                    context.Selection);

            var resolveResult = new CompleteValueResult(value, context.Field.Type);
            return resolveResult.CompleteValueAsync(context);
        }
    }
}