using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL;

public interface ISubscriberMiddleware
{
    ValueTask<ISubscriberResult> Invoke(Subscriber next, IResolverContext context, CancellationToken unsubscribe);
}