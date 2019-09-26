using System.Threading;
using System.Threading.Tasks;
using tanka.graphql.resolvers;

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