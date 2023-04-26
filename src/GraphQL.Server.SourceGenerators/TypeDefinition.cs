using System;

namespace Tanka.GraphQL.Server.SourceGenerators;

public abstract class TypeDefinition: IEquatable<TypeDefinition>
{
    public string? Namespace { get; init; }

    public string TargetType { get; init; }

    public bool Equals(TypeDefinition? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Namespace == other.Namespace 
               && TargetType == other.TargetType;
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
            return ((Namespace != null ? Namespace.GetHashCode() : 0) * 397) ^ TargetType.GetHashCode();
        }
    }
}