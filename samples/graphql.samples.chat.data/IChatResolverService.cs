using System.Threading;
using System.Threading.Tasks;
using tanka.graphql.resolvers;

namespace tanka.graphql.samples.chat.data
{
    public interface IChatResolverService
    {
        Task<IResolveResult> GetMessagesAsync(ResolverContext context);

        Task<IResolveResult> AddMessageAsync(ResolverContext context);

        Task<IResolveResult> EditMessageAsync(ResolverContext context);

        Task<ISubscribeResult> StreamMessagesAsync(ResolverContext context, CancellationToken cancellationToken);

        Task<IResolveResult> ResolveMessageAsync(ResolverContext context);
    }
}