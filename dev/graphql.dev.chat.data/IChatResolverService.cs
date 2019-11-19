using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Samples.Chat.Data
{
    public interface IChatResolverService
    {
        ValueTask<IResolveResult> GetMessagesAsync(IResolverContext context);

        ValueTask<IResolveResult> AddMessageAsync(IResolverContext context);

        ValueTask<IResolveResult> EditMessageAsync(IResolverContext context);

        ValueTask<ISubscribeResult> StreamMessagesAsync(IResolverContext context, CancellationToken cancellationToken);

        ValueTask<IResolveResult> ResolveMessageAsync(IResolverContext context);
    }
}