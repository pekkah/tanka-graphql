using System.Diagnostics;

namespace Tanka.GraphQL.ValueResolution;

public class SubscriberContext : ResolverContextBase
{
    public IAsyncEnumerable<object?>? ResolvedValue { get; set; }

    public void SetResult<T>(IAsyncEnumerable<T> stream)
    {
        ResolvedValue = Cast(stream);
    }

    [DebuggerStepThrough]
    private async IAsyncEnumerable<object?> Cast<T>(IAsyncEnumerable<T> source)
    {
        await foreach (var item in source)
        {
            yield return item;
        }
    }
}