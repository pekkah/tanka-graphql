using System;
using System.Collections.Generic;
using System.Linq;
using tanka.graphql.language;
using tanka.graphql.resolvers;
using tanka.graphql.type;

namespace tanka.graphql.schema
{
    public partial class ConnectionBuilder
    {
        public bool TryGetField(ComplexType owner, string fieldName, out IField field)
        {
            if (_fields.TryGetValue(owner.Name, out var fields))
                if (fields.TryGetValue(fieldName, out field))
                    return true;

            field = null;
            return false;
        }

        public bool TryGetInputField(InputObjectType owner, string fieldName, out InputObjectField field)
        {
            if (_inputFields.TryGetValue(owner.Name, out var fields))
                if (fields.TryGetValue(fieldName, out field))
                    return true;

            field = null;
            return false;
        }

        public IEnumerable<KeyValuePair<string, IField>> GetFields(ComplexType type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            if (!_fields.TryGetValue(type.Name, out var fields))
                return Enumerable.Empty<KeyValuePair<string, IField>>();

            return fields;
        }

        public IEnumerable<KeyValuePair<string, InputObjectField>> GetInputFields(InputObjectType type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            if (!_inputFields.TryGetValue(type.Name, out var fields))
                return Enumerable.Empty<KeyValuePair<string, InputObjectField>>();

            return fields;
        }

        public ResolverBuilder GetOrAddResolver(ComplexType type, string fieldName)
        {
            if (_resolvers.TryGetValue(type.Name, out var fields))
                if (fields.TryGetValue(fieldName, out var builder))
                    return builder;

            return Resolver(type, fieldName);
        }

        public SubscriberBuilder GetOrAddSubscriber(ComplexType type, string fieldName)
        {
            if (_subscribers.TryGetValue(type.Name, out var fields))
                if (fields.TryGetValue(fieldName, out var builder))
                    return builder;

            return Subscriber(type, fieldName);
        }

        public bool TryGetResolver(ComplexType type, string fieldName, out ResolverBuilder resolver)
        {
            if (_resolvers.TryGetValue(type.Name, out var fields))
                if (fields.TryGetValue(fieldName, out resolver))
                    return true;

            resolver = null;
            return false;
        }

        public bool TryGetSubscriber(ComplexType type, string fieldName, out SubscriberBuilder subscriber)
        {
            if (_subscribers.TryGetValue(type.Name, out var fields))
                if (fields.TryGetValue(fieldName, out subscriber))
                    return true;

            subscriber = null;
            return false;
        }

        internal IEnumerable<KeyValuePair<string, InputObjectField>> GetInputFields(string type)
        {
            if (Builder.TryGetType<InputObjectType>(type, out var inputType))
                return GetInputFields(inputType);

            throw new DocumentException(
                $"Input type '{type}' is not known by the builder.");
        }
    }
}