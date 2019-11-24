using System.Collections.Generic;

namespace Tanka.GraphQL.Introspection
{
    // ReSharper disable once InconsistentNaming
    public class __Schema
    {
        public List<__Type> Types { get; set; }

        public __Type QueryType { get; set; }

        public __Type MutationType { get; set; }

        public __Type SubscriptionType { get; set; }

        public List<__Directive> Directives { get; set; }
    }
}