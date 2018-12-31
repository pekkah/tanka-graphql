using System.Collections.Generic;
using tanka.graphql.resolvers;
using tanka.graphql.type;

namespace tanka.graphql
{
    public interface ISubscriberMap
    {
        Subscriber GetSubscriber(ComplexType type, KeyValuePair<string, IField> field);
    }
}