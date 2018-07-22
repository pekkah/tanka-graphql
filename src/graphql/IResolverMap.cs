using System.Threading.Tasks;
using fugu.graphql.resolvers;
using fugu.graphql.type;

namespace fugu.graphql
{
    public interface IResolverMap
    {
        Task<Resolver> GetResolverAsync(ResolverContext resolverContext);
    }
}