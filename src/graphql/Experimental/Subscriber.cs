using System.Threading;
using System.Threading.Tasks;

namespace Tanka.GraphQL.Experimental;

public delegate ValueTask Subscriber(
    SubscriberContext context,
    CancellationToken unsubscribe);