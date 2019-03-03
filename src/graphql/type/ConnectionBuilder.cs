using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace tanka.graphql.type
{
    public class ConnectionBuilder
    {
        private readonly Dictionary<string, Dictionary<string, IField>> _fields =
            new Dictionary<string, Dictionary<string, IField>>();

        private readonly Dictionary<string, Dictionary<string, InputObjectField>> _inputFields =
            new Dictionary<string, Dictionary<string, InputObjectField>>();

        public ConnectionBuilder(SchemaBuilder builder)
        {
            Builder = builder;
        }

        public SchemaBuilder Builder { get; }

        public ConnectionBuilder Field(
            ComplexType owner,
            string fieldName,
            IType to,
            string description = null,
            IEnumerable<DirectiveInstance> directives = null,
            params (string Name, IType Type, object DefaultValue, string Description)[] args)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            if (fieldName == null) throw new ArgumentNullException(nameof(fieldName));
            if (to == null) throw new ArgumentNullException(nameof(to));

            if (!Builder.TryGetType<ComplexType>(owner.Name, out  _))
            {
                throw new SchemaBuilderException(owner.Name,
                    $"Cannot add Field. Owner type {owner.Name} is not known for {fieldName}.");
            }

            var target = to.Unwrap();
            if (!Builder.TryGetType<INamedType>(target.Name, out  _))
            {
                throw new SchemaBuilderException(owner.Name,
                    $"Cannot add Field '{fieldName} to {owner.Name}'. Target type {target.Name} is not known.");
            }

            if (!_fields.ContainsKey(owner.Name))
                _fields[owner.Name] = new Dictionary<string, IField>();

            if (_fields[owner.Name].ContainsKey(fieldName))
            {
                throw new SchemaBuilderException(owner.Name,
                    $"Cannot add field '{fieldName}'. Type '{owner.Name}' already has field with same name.");
            }

            _fields[owner.Name].Add(fieldName, new Field(to, new Args(args),
                new Meta(description, directives: directives)));

            return this;
        }

        public ConnectionBuilder InputField(
            InputObjectType owner,
            string fieldName,
            IType to,
            object defaultValue = null,
            string description = null,
            IEnumerable<DirectiveInstance> directives = null)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            if (fieldName == null) throw new ArgumentNullException(nameof(fieldName));
            if (to == null) throw new ArgumentNullException(nameof(to));

            if (!Builder.TryGetType<InputObjectType>(owner.Name, out  _))
            {
                throw new SchemaBuilderException(owner.Name,
                    $"Cannot add InputField. Owner type {owner.Name} is not known for {fieldName}.");
            }

            var target = to.Unwrap();
            if (!Builder.TryGetType<INamedType>(target.Name, out  _))
            {
                throw new SchemaBuilderException(owner.Name,
                    $"Cannot add Field '{fieldName} to {owner.Name}'. Target type {target.Name} is not known.");
            }

            if (!_inputFields.ContainsKey(owner.Name))
                _inputFields[owner.Name] = new Dictionary<string, InputObjectField>();

            if (_inputFields[owner.Name].ContainsKey(fieldName))
            {
                throw new SchemaBuilderException(owner.Name,
                    $"Cannot add input field '{fieldName}'. Type '{owner.Name}' already has field with same name.");
            }

            _inputFields[owner.Name].Add(
                fieldName,
                new InputObjectField(to, new Meta(description, directives: directives), defaultValue));

            return this;
        }

        public ConnectionBuilder IncludeFields(ComplexType owner, IEnumerable<KeyValuePair<string, IField>> fields)
        {
            if (!Builder.TryGetType<ComplexType>(owner.Name, out  _))
            {
                throw new SchemaBuilderException(owner.Name,
                    $"Cannot include fields. Owner type {owner.Name} is not known.");
            }

            if (!_fields.ContainsKey(owner.Name))
                _fields[owner.Name] = new Dictionary<string, IField>();

            foreach (var field in fields)
            {
                _fields[owner.Name].Add(field.Key, field.Value);
            }

            return this;
        }

        public bool TryGetField(ComplexType owner, string fieldName, out IField field)
        {
            if (_fields.TryGetValue(owner.Name, out var fields))
                if (fields.TryGetValue(fieldName, out field))
                    return true;

            field = null;
            return false;
        }

        public ConnectionBuilder IncludeInputFields(InputObjectType owner,
            IEnumerable<KeyValuePair<string, InputObjectField>> fields)
        {
            if (!Builder.TryGetType<InputObjectType>(owner.Name, out  _))
            {
                throw new SchemaBuilderException(owner.Name,
                    $"Cannot include input fields. Owner type {owner.Name} is not known.");
            }

            foreach (var field in fields)
            {
                if (!_fields.ContainsKey(owner.Name))
                    _inputFields[owner.Name] = new Dictionary<string, InputObjectField>();

                _inputFields[owner.Name].Add(field.Key, field.Value);
            }

            return this;
        }

        public (Dictionary<string, Dictionary<string, IField>> Fields,
            Dictionary<string, Dictionary<string, InputObjectField>> InputFields) Build()
        {
            return (_fields, _inputFields);
        }

        public bool TryGetInputField(InputObjectType owner, string fieldName, out InputObjectField field)
        {
            if (_inputFields.TryGetValue(owner.Name, out var fields))
                if (fields.TryGetValue(fieldName, out field))
                    return true;

            field = null;
            return false;
        }
    }
}