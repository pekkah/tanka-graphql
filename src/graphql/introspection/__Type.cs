using System;
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

    public class __Type : IEquatable<__Type>
    {
        public override string ToString()
        {
            return $"{Kind} {Name}";
        }


        public __TypeKind? Kind { get; set; }

        public string Name { get;set; }

        public string Description { get; set; }

        public List<__Field> Fields { get;set; }

        public List<__Type> Interfaces { get;set; }

        public List<__Type> PossibleTypes { get; set; }

        public List<__EnumValue> EnumValues { get; set; }

        public List<__InputValue> InputFields { get; set; }

        public __Type OfType { get; set; }

        public bool Equals(__Type other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Kind == other.Kind && string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((__Type) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Kind.GetHashCode() * 397) ^ (Name != null ? Name.GetHashCode() : 0);
            }
        }

        public static bool operator ==(__Type left, __Type right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(__Type left, __Type right)
        {
            return !Equals(left, right);
        }
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