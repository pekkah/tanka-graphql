using tanka.graphql.tools;
using tanka.graphql.type;

namespace tanka.graphql
{
    public delegate SchemaVisitorBase SchemaVisitorFactory(ISchema schema, IResolverMap resolvers, ISubscriberMap subscribers = null);
}