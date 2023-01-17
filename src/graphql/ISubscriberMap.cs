using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL;

public interface ISubscriberMap
{
    Subscriber? GetSubscriber(string typeName, string fieldName);

    IEnumerable<(string TypeName, IEnumerable<string> Fields)> GetTypes();
}

public static class SubscriberMapExtensions
{
    public static Subscriber? GetSubscriber(this ISubscriberMap map, ObjectDefinition type,
        FieldDefinition field)
    {
        return map.GetSubscriber(type.Name, field.Name);
    }
}