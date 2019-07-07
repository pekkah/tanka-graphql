using System;
using System.Collections.Generic;
using System.Linq;
using tanka.graphql.type;

namespace tanka.graphql.schema
{
    public partial class SchemaBuilder
    {
        public IEnumerable<T> StreamTypes<T>() where T : INamedType
        {
            return _types.Values.OfType<T>();
        }

        public bool TryGetDirective(string name, out DirectiveType directiveType)
        {
            return _directives.TryGetValue(name, out directiveType);
        }

        public bool TryGetType<T>(string name, out T namedType)
            where T : INamedType
        {
            return _types.TryGetValue(name, out namedType);
        }
    }
}