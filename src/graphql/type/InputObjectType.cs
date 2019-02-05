using System;
using System.Collections.Generic;
using System.Linq;
using tanka.graphql.execution;

namespace tanka.graphql.type
{
    public class InputObjectType : INamedType, IDescribable
    {
        private readonly InputFields _fields = new InputFields();

        public InputObjectType(string name, InputFields fields, Meta meta = null)
        {
            Name = name;
            Meta = meta ?? new Meta(null);

            foreach (var field in fields)
            {
                var fieldType = field.Value.Type;

                if (!TypeIs.IsInputType(fieldType))
                    throw new InvalidOperationException(
                        $"Input type {name} cannot contain a non input type field {field.Key}");

                AddField(field.Key, field.Value);
            }
        }

        public IEnumerable<KeyValuePair<string, InputObjectField>> Fields => _fields;

        public Meta Meta { get; }

        public string Description => Meta.Description;

        public string Name { get; }

        public InputObjectField GetField(string name)
        {
            if (!_fields.ContainsKey(name))
                return null;

            return _fields[name];
        }

        public bool HasField(string name)
        {
            return _fields.ContainsKey(name);
        }

        public string GetFieldName(InputObjectField field)
        {
            var foundField = _fields.Single(f => f.Value == field).Key;
            return foundField;
        }

        public override string ToString()
        {
            return $"{Name}";
        }

        protected void AddField(string name, InputObjectField field)
        {
            if (HasField(name))
                throw new InvalidOperationException(
                    $"Cannot add field to type. Field {name} already exists.");

            _fields[name] = field;
        }
    }
}