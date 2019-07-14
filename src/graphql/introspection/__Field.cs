using System.Collections.Generic;

namespace tanka.graphql.introspection
{
    // ReSharper disable once InconsistentNaming
    public class __Field
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public List<__InputValue> Args { get; set; }

        public __Type Type { get; set; }

        public bool IsDeprecated { get; set; }

        public string DeprecationReason { get; set; }
    }
}