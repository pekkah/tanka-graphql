using tanka.graphql.type;

namespace tanka.graphql.schema
{
    public partial class ConnectionBuilder
    {
        public ConnectionBuilder RemoveField(ComplexType complexType, string fieldName)
        {
            if (_fields.TryGetValue(complexType.Name, out var fields))
                if (fields.ContainsKey(fieldName))
                    fields.Remove(fieldName);

            if (_resolvers.TryGetValue(complexType.Name, out var fieldResolvers))
                if (fieldResolvers.TryGetValue(fieldName, out _))
                    fieldResolvers.Remove(fieldName);

            if (_subscribers.TryGetValue(complexType.Name, out var fieldSubscribers))
                if (fieldSubscribers.TryGetValue(fieldName, out _))
                    fieldSubscribers.Remove(fieldName);

            return this;
        }

        public ConnectionBuilder RemoveField(InputObjectType inputObject, string fieldName)
        {
            if (_inputFields.TryGetValue(inputObject.Name, out var fields))
                if (fields.ContainsKey(fieldName))
                    fields.Remove(fieldName);

            return this;
        }
    }
}