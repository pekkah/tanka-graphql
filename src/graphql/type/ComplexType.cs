using System;
using System.Collections.Generic;
using System.Linq;

namespace tanka.graphql.type
{
    public abstract class ComplexType : IGraphQLType
    {
        private readonly Fields _fields = new Fields();

        public IEnumerable<KeyValuePair<string, IField>> Fields => _fields;

        public IField GetField(string name)
        {
            if (!_fields.ContainsKey(name))
                return null;

            return _fields[name];
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
                    $"Cannot add field to type. Field {name} already exists.");
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