using System.Collections.Generic;

namespace tanka.graphql.type
{
    public class EnumValues : Dictionary<string, Meta>
    {
        public EnumValues()
        {
        }

        public EnumValues(IEnumerable<string> values)
        {
            foreach (var value in values) this[value] = null;
        }

        public void Add(string name, string description = null, string depricationReason = null)
        {
            Add(name, new Meta(description, depricationReason));
        }
    }
}