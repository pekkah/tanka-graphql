using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tanka.GraphQL.Experimental;

public delegate ValueTask<IAsyncEnumerable<object?>> SubscriberMiddleware(
    ResolverContext context,
    CancellationToken unsubscribe, 
    Subscriber next);