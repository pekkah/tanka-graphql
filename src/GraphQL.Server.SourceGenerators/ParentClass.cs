using System;

namespace Tanka.GraphQL.Server.SourceGenerators;

public class ParentClass(string keyword, string name, string constraints, ParentClass? child)
    : IEquatable<ParentClass>
{
    public ParentClass? Child { get; } = child;

    public string Keyword { get; } = keyword;

    public string Name { get; } = name;

    public string Constraints { get; } = constraints;

    public bool Equals(ParentClass? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Equals(Child, other.Child)
               && Keyword == other.Keyword
               && Name == other.Name
               && Constraints == other.Constraints;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((ParentClass)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = (Child != null ? Child.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ Keyword.GetHashCode();
            hashCode = (hashCode * 397) ^ Name.GetHashCode();
            hashCode = (hashCode * 397) ^ Constraints.GetHashCode();
            return hashCode;
        }
    }
}