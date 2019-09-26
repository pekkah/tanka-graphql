using System;
using System.Collections.Generic;
using System.Linq;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.TypeSystem;
using ComplexType = Tanka.GraphQL.TypeSystem.ComplexType;
using IField = Tanka.GraphQL.TypeSystem.IField;
using InputObjectField = Tanka.GraphQL.TypeSystem.InputObjectField;
using InputObjectType = Tanka.GraphQL.TypeSystem.InputObjectType;

namespace Tanka.GraphQL.SchemaBuilding
{
    public partial class ConnectionBuilder
    {
        public bool TryGetField(TypeSystem.ComplexType owner, string fieldName, out TypeSystem.IField field)
        {
            if (_fields.TryGetValue(owner.Name, out var fields))
                if (fields.TryGetValue(fieldName, out field))
                    return true;

            field = null;
            return false;
        }

        public bool TryGetInputField(TypeSystem.InputObjectType owner, string fieldName, out TypeSystem.InputObjectField field)
        {
            if (_inputFields.TryGetValue(owner.Name, out var fields))
                if (fields.TryGetValue(fieldName, out field))
                    return true;

            field = null;
            return false;
        }

        public IEnumerable<KeyValuePair<string, TypeSystem.IField>> GetFields(TypeSystem.ComplexType type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            if (!_fields.TryGetValue(type.Name, out var fields))
                return Enumerable.Empty<KeyValuePair<string, TypeSystem.IField>>();

            return fields;
        }

        public IEnumerable<KeyValuePair<string, TypeSystem.InputObjectField>> GetInputFields(TypeSystem.InputObjectType type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            if (!_inputFields.TryGetValue(type.Name, out var fields))
                return Enumerable.Empty<KeyValuePair<string, TypeSystem.InputObjectField>>();

            return fields;
        }

        public ResolverBuilder GetOrAddResolver(TypeSystem.ComplexType type, string fieldName)
        {
            if (_resolvers.TryGetValue(type.Name, out var fields))
                if (fields.TryGetValue(fieldName, out var builder))
                    return builder;

            return Resolver(type, fieldName);
        }

        public SubscriberBuilder GetOrAddSubscriber(TypeSystem.ComplexType type, string fieldName)
        {
            if (_subscribers.TryGetValue(type.Name, out var fields))
                if (fields.TryGetValue(fieldName, out var builder))
                    return builder;

            return Subscriber(type, fieldName);
        }

        public bool TryGetResolver(TypeSystem.ComplexType type, string fieldName, out ResolverBuilder resolver)
        {
            if (_resolvers.TryGetValue(type.Name, out var fields))
                if (fields.TryGetValue(fieldName, out resolver))
                    return true;

            resolver = null;
            return false;
        }

        public bool TryGetSubscriber(TypeSystem.ComplexType type, string fieldName, out SubscriberBuilder subscriber)
        {
            if (_subscribers.TryGetValue(type.Name, out var fields))
                if (fields.TryGetValue(fieldName, out subscriber))
                    return true;

            subscriber = null;
            return false;
        }

        internal IEnumerable<KeyValuePair<string, TypeSystem.InputObjectField>> GetInputFields(string type)
        {
            if (Builder.TryGetType<TypeSystem.InputObjectType>(type, out var inputType))
                return GetInputFields(inputType);

            throw new DocumentException(
                $"Input type '{type}' is not known by the builder.");
        }
    }
}