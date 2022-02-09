using System.Threading.Tasks;

namespace Tanka.GraphQL.ValueResolution
{
    public interface IResolverResult
    {
        object? Value { get; }

        ValueTask<object?> CompleteValueAsync(
            IResolverContext context);
    }
}