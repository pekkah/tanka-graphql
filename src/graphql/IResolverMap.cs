using System.Collections.Generic;
using tanka.graphql.resolvers;
using tanka.graphql.type;

namespace tanka.graphql
{
    public interface IResolverMap
    {
        Resolver GetResolver(ComplexType type, KeyValuePair<string, IField> field);
    }
}