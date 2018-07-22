using fugu.graphql.tools;
using fugu.graphql.type;

namespace fugu.graphql
{
    public delegate SchemaVisitorBase SchemaVisitorFactory(ISchema schema, IResolverMap resolvers, ISubscriberMap subscribers = null);
}