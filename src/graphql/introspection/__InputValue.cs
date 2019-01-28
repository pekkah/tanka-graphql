using System;
using tanka.graphql.type;

namespace tanka.graphql.introspection
{
    public class __InputValue
    {
        public string Name { get; set; }

        public string Description { get;set; }

        public IType Type { get; set; }

        public string DefaultValue { get; set; }
    }
}