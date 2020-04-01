using System.Collections.Generic;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.SchemaBuilding
{
    public class EnumValuesBuilder
    {
        private readonly EnumValues _values = new EnumValues();

        public EnumValuesBuilder Value(
            string value,
            string? description,
            IEnumerable<DirectiveInstance> directives,
            string? deprecationReason)
        {
            _values.Add(value, description, directives, deprecationReason);
            return this;
        }

        public EnumValues Build()
        {
            return _values;
        }
    }
}