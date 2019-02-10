using System;
using System.Collections.Generic;
using static tanka.graphql.graph.Wrapper;

namespace tanka.graphql.type
{
    public abstract class ComplexType : INamedType
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
                    $"Cannot add field to type '{Name}'. Field '{name}' already exists.");
            }

            if (field.Type.Unwrap() is SelfReferenceType)
            {
                var actualType = WrapIfRequired(field.Type, this);
                _fields[name] = new SelfReferenceField(
                    actualType, 
                    new Args(field.Arguments), 
                    field.Meta);

                return;
            }

            if (field is SelfReferenceField selfReference)
            {
                var actualType = WrapIfRequired(selfReference.Type, this);
                _fields[name] = new SelfReferenceField(
                    actualType,
                    new Args(field.Arguments),
                    field.Meta);
            }

            _fields[name] = field;
        }

        public abstract string Name { get; }
    }
}