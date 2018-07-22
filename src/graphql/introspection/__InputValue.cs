using System;
using fugu.graphql.type;

namespace fugu.graphql.introspection
{
    public class __InputValue
    {
        public string Name { get; set; }

        public string Description { get;set; }

        public IGraphQLType Type { get; set; }

        public string DefaultValue { get; set; }
    }
}