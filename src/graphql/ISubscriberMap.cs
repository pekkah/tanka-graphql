using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL
{
    public interface ISubscriberMap
    {
        Subscriber? GetSubscriber(string typeName, string fieldName);
    }

    public static class SubscriberMapExtensions
    {
        public static Subscriber? GetSubscriber(this ISubscriberMap map, ObjectDefinition type,
            FieldDefinition field)
        {
            return map.GetSubscriber(type.Name, field.Name);
        }
    }
}