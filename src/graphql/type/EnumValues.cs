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

    public class EnumValue : IDescribable, IDeprecable
    {
        public EnumValue(string description, IEnumerable<DirectiveInstance> directives = null, string deprecationReason = null)
        {
            Description = description ?? string.Empty;
            DeprecationReason = deprecationReason;
            Directives = directives;
        }

        public string Description { get; }

        public string DeprecationReason { get; }

        public IEnumerable<DirectiveInstance> Directives { get; }

        public bool IsDeprecated => !string.IsNullOrEmpty(DeprecationReason);
    }
}