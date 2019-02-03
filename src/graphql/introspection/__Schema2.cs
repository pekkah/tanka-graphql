using System.Collections.Generic;
using System.Linq;

namespace tanka.graphql.introspection
{
    public class __Schema2
    {
        public string QueryTypeName { get; set; }

        public string MutationTypeName { get; set; }

        public string SubscriptionTypeName { get; set; }

        public IEnumerable<string> DirectiveNames { get; set; } = Enumerable.Empty<string>();
    }
}