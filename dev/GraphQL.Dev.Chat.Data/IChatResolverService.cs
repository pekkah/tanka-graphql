using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Samples.Chat.Data;

public interface IChatResolverService
{
    ValueTask GetMessagesAsync(ResolverContext context);

    ValueTask AddMessageAsync(ResolverContext context);

    ValueTask EditMessageAsync(ResolverContext context);

    ValueTask StreamMessagesAsync(SubscriberContext context, CancellationToken cancellationToken);

    ValueTask ResolveMessageAsync(ResolverContext context);
}