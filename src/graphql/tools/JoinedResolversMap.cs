using System.Collections.Generic;
using System.Linq;
using fugu.graphql.resolvers;
using fugu.graphql.type;

namespace fugu.graphql.tools
{
    internal class JoinedResolversMap : IResolverMap
    {
        private readonly List<IResolverMap> _resolversMaps;

        public JoinedResolversMap(IResolverMap[] resolverMaps)
        {
            _resolversMaps = resolverMaps.ToList();
        }

        public Resolver GetResolver(ComplexType type, KeyValuePair<string, IField> field)
        {
            foreach (var resolverMap in _resolversMaps)
            {
                var resolver = resolverMap.GetResolver(type, field);

                if (resolver == null)
                    continue;

                return resolver;
            }

            return null;
        }
    }
}