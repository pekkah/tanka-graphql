using System.Threading.Tasks;
using fugu.graphql.resolvers;
using fugu.graphql.type;

namespace fugu.graphql
{
    public interface ISubscriberMap
    {
        Task<Subscriber> GetSubscriberAsync(ResolverContext resolverContext);
    }
}