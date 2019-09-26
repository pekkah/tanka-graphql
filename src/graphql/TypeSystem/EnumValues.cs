using System.Collections.Generic;

namespace Tanka.GraphQL.TypeSystem
{
    public class EnumValues : Dictionary<string, EnumValue>
    {
        public EnumValues(params EnumValue[] values)
        {
            foreach (var value in values) Add(value.Value, value);
        }

        public EnumValues()
        {
        }

        public void Add(
            string value,
            string description = null,
            IEnumerable<DirectiveInstance> directives = null,
            string deprecationReason = null)
        {
            Add(value, new EnumValue(value, description, directives, deprecationReason));
        }
    }
}