using System;
using System.Collections.Generic;
using System.Linq;
using tanka.graphql.execution;

namespace tanka.graphql.type
{
    public class InputObjectType : IGraphQLType
    {
        private readonly InputFields _fields = new InputFields();

        public IEnumerable<KeyValuePair<string, InputObjectField>> Fields => _fields;

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

        protected void AddField(string name, InputObjectField field)
        {
            if (HasField(name))
            {
                throw new InvalidOperationException(
                    $"Cannot add field to type. Field {name} already exists.");
            }

            _fields[name] = field;
        }

        public string GetFieldName(InputObjectField field)
        {
            var foundField = _fields.Single(f => f.Value == field).Key;
            return foundField;
        }

        public InputObjectType(string name, InputFields fields, Meta meta = null)
        {
            Name = name;
            Meta = meta ?? new Meta(null);

            foreach (var field in fields)
            {
                var fieldType = field.Value.Type;

                if (!Validations.IsInputType(fieldType))
                    throw new InvalidOperationException(
                        $"Input type {name} cannot contain a non input type field {field.Key}");

                AddField(field.Key, field.Value);
            }
        }

        public Meta Meta { get; }

        public string Name { get; }

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}