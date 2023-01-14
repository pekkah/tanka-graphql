using System.Threading;
using System.Threading.Tasks;

namespace Tanka.GraphQL.Experimental;

public delegate ValueTask SubscriberMiddleware(
    SubscriberContext context,
    CancellationToken unsubscribe,
    Subscriber next);