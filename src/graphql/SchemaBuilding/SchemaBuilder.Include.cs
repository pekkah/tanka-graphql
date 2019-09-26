using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.SchemaBuilding
{
    public partial class SchemaBuilder
    {
        public SchemaBuilder Include(DirectiveType directiveType)
        {
            if (_directives.ContainsKey(directiveType.Name))
                throw new SchemaBuilderException(directiveType.Name,
                    $"Cannot include directive '{directiveType.Name}'. Directive already known.");

            _directives.Add(directiveType.Name, directiveType);
            return this;
        }

        public SchemaBuilder Include(INamedType type)
        {
            if (_types.ContainsKey(type.Name))
                throw new SchemaBuilderException(type.Name,
                    $"Cannot include type '{type.Name}'. Type already known.");

            _types.Add(type.Name, type);
            return this;
        }
    }
}