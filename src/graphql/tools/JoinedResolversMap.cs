using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using fugu.graphql.resolvers;

namespace fugu.graphql.tools
{
    internal class JoinedResolversMap : IResolverMap
    {
        private readonly List<IResolverMap> _resolversMaps;

        public JoinedResolversMap(IResolverMap[] resolverMaps)
        {
            _resolversMaps = resolverMaps.ToList();
        }

        public async Task<Resolver> GetResolverAsync(ResolverContext resolverContext)
        {
            foreach (var resolverMap in _resolversMaps)
            {
                var resolver = await resolverMap.GetResolverAsync(resolverContext);

                if (resolver == null)
                    continue;

                return resolver;
            }

            return null;
        }
    }
}