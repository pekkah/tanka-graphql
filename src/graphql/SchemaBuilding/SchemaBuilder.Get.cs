using System.Collections.Generic;
using System.Linq;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.TypeSystem.ValueSerialization;
using DirectiveType = Tanka.GraphQL.TypeSystem.DirectiveType;
using INamedType = Tanka.GraphQL.TypeSystem.INamedType;

namespace Tanka.GraphQL.SchemaBuilding
{
    public partial class SchemaBuilder
    {
        public IEnumerable<T> GetTypes<T>() where T : INamedType
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

        public IValueConverter GetValueConverter(string name)
        {
            if (_valueConverters.TryGetValue(name, out var converter))
                return converter;

            throw new SchemaBuilderException(
                name,
                $"Could not get value converter for type '{name}'");
        }

        public bool TryGetValueConverter(string name, out IValueConverter serializer)
        {
            return _valueConverters.TryGetValue(name, out serializer);
        }
    }
}