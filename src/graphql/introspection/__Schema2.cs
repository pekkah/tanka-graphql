using System.Collections.Generic;
using System.Linq;
using tanka.graphql.type;

namespace tanka.graphql.introspection
{
    public class Introspect
    {
        public static __Schema2 Schema(Schema schema)
        {
            return new __Schema2()
            {
                QueryTypeName = schema.Query.Name,
                MutationTypeName = schema.Mutation?.Name,
                SubscriptionTypeName = schema.Subscription?.Name,
                DirectiveNames = schema.Directives.Select(d => d.Name)
                    .ToList()
            };
        }
    }

    public class __Schema2
    {
        public string QueryTypeName { get; set; }

        public string MutationTypeName { get; set; }

        public string SubscriptionTypeName { get; set; }

        public IEnumerable<string> DirectiveNames { get; set; } = Enumerable.Empty<string>();
    }
}