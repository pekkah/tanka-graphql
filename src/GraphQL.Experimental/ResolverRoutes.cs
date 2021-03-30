using System;
using System.Collections;
using System.Collections.Generic;

namespace Tanka.GraphQL.Experimental
{
    public class ResolverRoutes: IEnumerable<ResolveFieldValue>
    {
        private readonly Dictionary<string, FieldRoutes> _objectRoutes = new();

        public ResolveFieldValue? Resolver(string path)
        {
            var parts = path.Split('.');

            if (parts.Length != 2)
                throw new ArgumentOutOfRangeException(nameof(path));

            var objectName = parts[0];
            var fieldName = parts[1];

            if (_objectRoutes.TryGetValue(objectName, out var fields))
                return fields.Resolver(fieldName);

            return null;
        }

        public void Add(string path, ResolveFieldValue resolver)
        {
            var parts = path.Split('.');

            if (parts.Length != 2)
                throw new ArgumentOutOfRangeException(nameof(path));

            var objectName = parts[0];
            var fieldName = parts[1];

            if (!_objectRoutes.TryGetValue(objectName, out var fields))
            {
                fields = new FieldRoutes();
                _objectRoutes[objectName] = fields;
            }

            fields.Add(fieldName, resolver);
        }

        internal class FieldRoutes
        {
            private readonly Dictionary<string, ResolveFieldValue> _fieldRoutes = new();

            public ResolveFieldValue? Resolver(string fieldName)
            {
                if (_fieldRoutes.TryGetValue(fieldName, out var resolver))
                    return resolver;

                return null;
            }

            public ResolveFieldEventStream? Subscriber(string fieldName)
            {
                return null;
            }

            public void Add(string fieldName, ResolveFieldValue resolver)
            {
                _fieldRoutes[fieldName] = resolver;
            }
        }

        public IEnumerator<ResolveFieldValue> GetEnumerator()
        {
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}