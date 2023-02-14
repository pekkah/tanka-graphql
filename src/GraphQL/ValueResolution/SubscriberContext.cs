namespace Tanka.GraphQL.ValueResolution;

public class SubscriberContext : ResolverContextBase
{
    public IAsyncEnumerable<object?>? ResolvedValue { get; set; }
}