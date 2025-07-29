using System;

namespace Tanka.GraphQL.Server.SourceGenerators;

public abstract class TypeDefinition : IEquatable<TypeDefinition>
{
    public string? Namespace { get; init; }

    public required string TargetType { get; init; }

    public string? GraphQLName { get; init; }

    public bool Equals(TypeDefinition? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Namespace == other.Namespace && TargetType == other.TargetType && GraphQLName == other.GraphQLName;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((TypeDefinition)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = (Namespace != null ? Namespace.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ TargetType.GetHashCode();
            hashCode = (hashCode * 397) ^ (GraphQLName != null ? GraphQLName.GetHashCode() : 0);
            return hashCode;
        }
    }
}