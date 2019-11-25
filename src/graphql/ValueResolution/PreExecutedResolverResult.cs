using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tanka.GraphQL.ValueResolution
{
    public class PreExecutedResolverResult : IResolverResult
    {
        private readonly IDictionary<string, object> _data;

        public PreExecutedResolverResult(IDictionary<string, object> data)
        {
            _data = data;
        }

        public object Value => _data;

        public ValueTask<object> CompleteValueAsync(IResolverContext context)
        {
            var value = _data[context.FieldName];
            var resolveResult = new CompleteValueResult(value, null);
            return resolveResult.CompleteValueAsync(context);
        }
    }
}