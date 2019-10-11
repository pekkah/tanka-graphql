using System;
using System.Collections.Generic;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.SchemaBuilding
{
    public partial class ConnectionBuilder
    {
        public ConnectionBuilder Include(
            ComplexType owner,
            IEnumerable<KeyValuePair<string, IField>> fields)
        {
            EnsureTypeKnown(owner);

            if (!_fields.ContainsKey(owner.Name))
                _fields[owner.Name] = new Dictionary<string, IField>();

            foreach (var field in fields)
                Include(owner, field);

            return this;
        }

        public ConnectionBuilder Include(
            ComplexType owner,
            KeyValuePair<string, IField> field)
        {
            EnsureTypeKnown(owner);
            EnsureTypeKnown(field.Value.Type);

            foreach (var argument in field.Value.Arguments)
            {
                EnsureTypeKnown(argument.Value.Type);
            }

            foreach (var directive in field.Value.Directives)
            {
                EnsureDirectiveKnown(directive);
            }

            if (!_fields.ContainsKey(owner.Name))
                _fields[owner.Name] = new Dictionary<string, IField>();

            _fields[owner.Name].Add(field.Key, field.Value);


            return this;
        }

        public ConnectionBuilder Include(
            InputObjectType owner,
            IEnumerable<KeyValuePair<string, InputObjectField>> fields)
        {
            EnsureTypeKnown(owner);

            if (!_inputFields.ContainsKey(owner.Name))
                _inputFields[owner.Name] = new Dictionary<string, InputObjectField>();

            foreach (var field in fields)
                Include(owner, field);

            return this;
        }

        public ConnectionBuilder Include(
            InputObjectType owner,
            KeyValuePair<string, InputObjectField> field)
        {
            EnsureTypeKnown(owner);
            EnsureTypeKnown(field.Value.Type);

            foreach (var directive in field.Value.Directives)
            {
                EnsureDirectiveKnown(directive);
            }

            if (!_inputFields.ContainsKey(owner.Name))
                _inputFields[owner.Name] = new Dictionary<string, InputObjectField>();

            _inputFields[owner.Name].Add(field.Key, field.Value);

            return this;
        }

        public ConnectionBuilder Include(
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

        public ConnectionBuilder Include(
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