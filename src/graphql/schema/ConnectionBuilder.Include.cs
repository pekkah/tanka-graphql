using System;
using System.Collections.Generic;
using tanka.graphql.resolvers;
using tanka.graphql.type;

namespace tanka.graphql.schema
{
    public partial class ConnectionBuilder
    {
        public ConnectionBuilder IncludeFields(
            ComplexType owner,
            IEnumerable<KeyValuePair<string, IField>> fields)
        {
            if (!Builder.TryGetType<ComplexType>(owner.Name, out _))
                throw new SchemaBuilderException(owner.Name,
                    $"Cannot include fields. Owner type {owner.Name} is not known.");

            if (!_fields.ContainsKey(owner.Name))
                _fields[owner.Name] = new Dictionary<string, IField>();

            foreach (var field in fields)
                _fields[owner.Name].Add(field.Key, field.Value);

            return this;
        }

        public ConnectionBuilder IncludeInputFields(
            InputObjectType owner,
            IEnumerable<KeyValuePair<string, InputObjectField>> fields)
        {
            if (!Builder.TryGetType<InputObjectType>(owner.Name, out _))
                throw new SchemaBuilderException(owner.Name,
                    $"Cannot include input fields. Owner type {owner.Name} is not known.");

            if (!_inputFields.ContainsKey(owner.Name))
                _inputFields[owner.Name] = new Dictionary<string, InputObjectField>();

            foreach (var field in fields) _inputFields[owner.Name].Add(field.Key, field.Value);

            return this;
        }

        public ConnectionBuilder IncludeResolver(
            ObjectType objectType,
            string fieldName,
            ResolverBuilder resolver)
        {
            if (_resolvers.TryGetValue(objectType.Name, out var fieldResolvers))
                fieldResolvers.Add(fieldName, resolver);
            else
                throw new ArgumentOutOfRangeException(nameof(objectType),
                    $"Cannot include resolver. Unknown type '{objectType.Name}'.");

            return this;
        }

        public ConnectionBuilder IncludeSubscriber(
            ObjectType objectType,
            string fieldName,
            SubscriberBuilder subscriber)
        {
            if (_subscribers.TryGetValue(objectType.Name, out var subscriberBuilders))
                subscriberBuilders.Add(fieldName, subscriber);
            else
                throw new ArgumentOutOfRangeException(nameof(objectType),
                    $"Cannot include subscriber. Unknown type '{objectType.Name}'.");

            return this;
        }
    }
}