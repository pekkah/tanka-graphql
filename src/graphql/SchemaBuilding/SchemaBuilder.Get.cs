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

        public IValueConverter GetScalarSerializer(string name)
        {
            if (_scalarSerializers.TryGetValue(name, out var serializer))
                return serializer;

            throw new SchemaBuilderException(
                name,
                $"Could not get serializer for type '{name}'");
        }
    }
}