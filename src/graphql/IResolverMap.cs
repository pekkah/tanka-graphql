using System.Collections.Generic;
using fugu.graphql.resolvers;
using fugu.graphql.type;

namespace fugu.graphql
{
    public interface IResolverMap
    {
        Resolver GetResolver(ComplexType type, KeyValuePair<string, IField> field);
    }
}