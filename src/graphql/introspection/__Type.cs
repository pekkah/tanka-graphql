using System.Collections.Generic;

namespace tanka.graphql.introspection
{
    public class __Schema
    {
        public List<__Type> Types { get; set; }

        public __Type QueryType { get; set; }

        public __Type MutationType { get; set; }

        public __Type SubscriptionType { get; set; }

        public List<__Directive> Directives { get; set; }
    }

    public class __Type
    {
        public __TypeKind? Kind { get; set; }

        public string Name { get;set; }

        public string Description { get; set; }

        public List<__Field> Fields { get;set; }

        public List<__Type> Interfaces { get;set; }

        public List<__Type> PossibleTypes { get; set; }

        public List<__EnumValue> EnumValues { get; set; }

        public List<__InputValue> InputFields { get; set; }

        public __Type OfType { get; set; }
    }

    public class __Field
    {
        public string Name { get;set; }

        public string Description { get; set; }

        public List<__InputValue> Args { get; set; }

        public __Type Type { get; set; }

        public bool IsDeprecated { get; set; }

        public string DeprecationReason { get; set; }
    }

    public class __EnumValue
    {
        public string Name { get;set; }

        public string Description { get; set; }

        public bool IsDeprecated { get; set; }

        public string DeprecationReason { get; set; }
    }

    public class __InputValue
    {
        public string Name { get;set; }

        public string Description { get; set; }

        public __Type Type { get; set; }

        public string DefaultValue { get; set; }
    }

    public class __Directive
    {
        public string Name { get;set; }

        public string Description { get; set; }

        public List<__DirectiveLocation> Locations { get; set; }

        public List<__InputValue> Args { get; set; }
    }
}