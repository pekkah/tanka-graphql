using System.Collections.Generic;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL
{
    public interface ISubscriberMap
    {
        Subscriber GetSubscriber(string typeName, string fieldName);
    }

    public static class SubscriberMapExtensions
    {
        public static Subscriber GetSubscriber(this ISubscriberMap map, ComplexType type,
            KeyValuePair<string, IField> field)
        {
            return map.GetSubscriber(type.Name, field.Key);
        }
    }
}