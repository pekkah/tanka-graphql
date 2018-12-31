using System.Collections.Generic;

namespace tanka.graphql.introspection
{
    public class __Directive
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public List<__DirectiveLocation> Locations { get; set; }

        public List<__InputValue> Args { get; set; }
    }
}