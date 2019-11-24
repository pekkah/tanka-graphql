using System.Threading.Tasks;

namespace Tanka.GraphQL.ValueResolution
{
    public interface IResolverResult2
    {
        ValueTask<object> CompleteValueAsync(
            IResolverContext context);
    }

    public class ResolverResultBase : IResolverResult2
    {
        private readonly object _value;

        public ResolverResultBase(object value)
        {
            _value = value;
        }

        public ValueTask<object> CompleteValueAsync(IResolverContext context)
        {
            return default;
        }
    }
}