using System;
using System.Collections.Generic;
using tanka.graphql.type;

namespace tanka.graphql.introspection
{
    public class __Field
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public List<__InputValue> Args { get; set; }

        public IType Type { get; set; }

        public bool IsDeprecated { get; set; }

        public string DeprecationReason { get; set; }
    }
}