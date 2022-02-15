using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Samples.Chat.Data;

public interface IChatResolverService
{
    ValueTask<IResolverResult> GetMessagesAsync(IResolverContext context);

    ValueTask<IResolverResult> AddMessageAsync(IResolverContext context);

    ValueTask<IResolverResult> EditMessageAsync(IResolverContext context);

    ValueTask<ISubscriberResult> StreamMessagesAsync(IResolverContext context, CancellationToken cancellationToken);

    ValueTask<IResolverResult> ResolveMessageAsync(IResolverContext context);
}