using System;
using System.Collections.Generic;
using System.Linq;

namespace Tanka.GraphQL.Server.SourceGenerators;

public class InputTypeDefinition: TypeDefinition, IEquatable<InputTypeDefinition>
{
    public string? Namespace { get; init; }

    public string TargetType { get; init; }

    public List<ObjectPropertyDefinition> Properties { get; set; } = new List<ObjectPropertyDefinition>();

    public ParentClass? ParentClass { get; set; }

    public bool Equals(InputTypeDefinition? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Namespace == other.Namespace 
               && TargetType == other.TargetType 
               && Properties.SequenceEqual(other.Properties) 
               && Equals(ParentClass, other.ParentClass);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((InputTypeDefinition)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = (Namespace != null ? Namespace.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ TargetType.GetHashCode();
            hashCode = (hashCode * 397) ^ Properties.GetHashCode();
            hashCode = (hashCode * 397) ^ (ParentClass != null ? ParentClass.GetHashCode() : 0);
            return hashCode;
        }
    }
}