using System;
using System.Collections.Generic;
using System.Linq;

namespace Tanka.GraphQL.Server.SourceGenerators;

public class InterfaceControllerDefinition : TypeDefinition, IEquatable<InterfaceControllerDefinition>
{
    public List<ObjectPropertyDefinition> Properties { get; init; } = [];

    public List<ObjectMethodDefinition> Methods { get; init; } = [];

    public ParentClass? ParentClass { get; init; }

    public bool IsStatic { get; init; }

    public IReadOnlyList<string> Usings { get; init; } = [];

    public bool Equals(InterfaceControllerDefinition? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return base.Equals(other) 
               && Properties.SequenceEqual(other.Properties) 
               && Methods.SequenceEqual(other.Methods) 
               && ParentClass?.Equals(other.ParentClass) == true
               && IsStatic == other.IsStatic 
               && Usings.SequenceEqual(other.Usings);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((InterfaceControllerDefinition)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = base.GetHashCode();
            hashCode = (hashCode * 397) ^ Properties.GetHashCode();
            hashCode = (hashCode * 397) ^ Methods.GetHashCode();
            hashCode = (hashCode * 397) ^ (ParentClass != null ? ParentClass.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ IsStatic.GetHashCode();
            hashCode = (hashCode * 397) ^ Usings.GetHashCode();
            return hashCode;
        }
    }
}