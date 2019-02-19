using System.Collections.Generic;

namespace tanka.graphql.type
{
    public class EnumValues : Dictionary<string, Meta>
    {
        public EnumValues(IEnumerable<string> values)
        {
            foreach (var value in values) this[value] = null;
        }

        public EnumValues(params (string value, string description, IEnumerable<DirectiveInstance> directives,  string deprecationReson)[] values)
        {
            foreach (var (value, description, directives, deprecationReson) in values)
            {
                Add(value, new Meta(description, deprecationReson, directives));
            }
        }

        public void Add(string name, string description = null, string deprecationReason = null)
        {
            Add(name, new Meta(description, deprecationReason));
        }
    }
}