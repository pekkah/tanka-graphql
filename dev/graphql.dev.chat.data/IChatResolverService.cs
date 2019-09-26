using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Samples.Chat.Data
{
    public interface IChatResolverService
    {
        ValueTask<IResolveResult> GetMessagesAsync(ResolverContext context);

        ValueTask<IResolveResult> AddMessageAsync(ResolverContext context);

        ValueTask<IResolveResult> EditMessageAsync(ResolverContext context);

        ValueTask<ISubscribeResult> StreamMessagesAsync(ResolverContext context, CancellationToken cancellationToken);

        ValueTask<IResolveResult> ResolveMessageAsync(ResolverContext context);
    }
}