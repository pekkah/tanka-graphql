using System.Collections.Generic;

namespace tanka.graphql.type
{
    public class EnumValues : Dictionary<string, EnumValue>
    {
        public EnumValues(IEnumerable<string> values)
        {
            foreach (var value in values) this[value] = null;
        }

        public EnumValues(params (string value, string description, IEnumerable<DirectiveInstance> directives,  string deprecationReson)[] values)
        {
            foreach (var (value, description, directives, deprecationReason) in values)
            {
                Add(value, new EnumValue(description, directives, deprecationReason));
            }
        }

        public void Add(string name, string description = null, string deprecationReason = null)
        {
            Add(name, new EnumValue(description, deprecationReason: deprecationReason));
        }
    }
}