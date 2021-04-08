using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.Experimental.Definitions;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

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

        public async ValueTask<(object? Value, ResolveAbstractType? ResolveAbstractType)> Resolve(
            OperationContext context, 
            ObjectDefinition objectdefinition, 
            object? objectvalue, 
            Name fieldname, 
            IReadOnlyDictionary<string, object?> coercedargumentvalues, 
            NodePath path, 
            CancellationToken cancellationtoken)
        {
            var resolver = Resolver($"{objectdefinition.Name}.{fieldname}");

            if (resolver == null)
                throw new Exception($"Missing resolver for '{objectdefinition.Name}.{fieldname}'.");

            return await resolver(
                context,
                objectdefinition, 
                objectvalue, 
                fieldname, 
                coercedargumentvalues, 
                path,
                cancellationtoken);
        }
    }
}