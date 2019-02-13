using System.Collections.Generic;

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
            if (!_fields.ContainsKey(owner.Name))
                _fields[owner.Name] = new Dictionary<string, IField>();

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
            if (!_inputFields.ContainsKey(owner.Name))
                _inputFields[owner.Name] = new Dictionary<string, InputObjectField>();

            _inputFields[owner.Name].Add(
                fieldName,
                new InputObjectField(to, new Meta(description, directives: directives), defaultValue));

            return this;
        }

        public ConnectionBuilder IncludeFields(ComplexType owner, IEnumerable<KeyValuePair<string, IField>> fields)
        {
            foreach (var field in fields)
            {
                if (!_fields.ContainsKey(owner.Name))
                    _fields[owner.Name] = new Dictionary<string, IField>();

                _fields[owner.Name].Add(field.Key, field.Value);
            }

            return this;
        }

        public bool IsPredefinedField(ComplexType owner, string fieldName, out IField field)
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
    }
}