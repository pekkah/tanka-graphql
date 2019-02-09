using System;
using System.Collections.Generic;
using System.Linq;

namespace tanka.graphql.type
{
    public abstract class ComplexType : INamedType
    {
        private readonly Fields _fields = new Fields();

        public IEnumerable<KeyValuePair<string, IField>> Fields => _fields
            .Select(HandleSpecialField);

        //todo(pekka): this is just a quick hack to make it work. Handle this when creating type.
        private KeyValuePair<string, IField> HandleSpecialField(KeyValuePair<string, IField> field)
        {
            if (field.Value is SelfReferenceField)
                return new KeyValuePair<string, IField>(
                    field.Key,
                    new Field(
                        this,
                        new Args(field.Value.Arguments),
                        field.Value.Meta));

            return field;
        }

        public IField GetField(string name)
        {
            if (!_fields.ContainsKey(name))
                return null;

            return GetFieldWithKey(name).Value;
        }

        public KeyValuePair<string, IField> GetFieldWithKey(string name)
        {
            return Fields.SingleOrDefault(f => f.Key == name);
        }

        public bool HasField(string name)
        {
            return _fields.ContainsKey(name);
        }

        protected void AddField(string name, IField field)
        {
            if (HasField(name))
            {
                throw new InvalidOperationException(
                    $"Cannot add field to type '{Name}'. Field '{name}' already exists.");
            }

            _fields[name] = field;
        }

        public string GetFieldName(IField field)
        {
            var foundField = _fields.Single(f => f.Value == field).Key;
            return foundField;
        }

        public abstract string Name { get; }
    }
}