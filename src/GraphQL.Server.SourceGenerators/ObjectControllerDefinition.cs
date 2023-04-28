using System;
using System.Collections.Generic;
using System.Linq;

namespace Tanka.GraphQL.Server.SourceGenerators;

public class ObjectControllerDefinition: TypeDefinition, IEquatable<ObjectControllerDefinition>
{
    public string? Namespace { get; init; }

    public string TargetType { get; init; }

    public List<ObjectPropertyDefinition> Properties { get; set; } = new List<ObjectPropertyDefinition>();

    public List<ObjectMethodDefinition>  Methods { get; set;  } = new List<ObjectMethodDefinition>();
       
    public ParentClass? ParentClass { get; set;  }

    public bool IsStatic { get; init; }

    public IReadOnlyList<string> Usings { get; init; }

    public virtual bool Equals(ObjectControllerDefinition? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return Namespace == other.Namespace
               && TargetType == other.TargetType
               && Properties.SequenceEqual(other.Properties, EqualityComparer<ObjectPropertyDefinition>.Default)
               && Methods.SequenceEqual(other.Methods, EqualityComparer<ObjectMethodDefinition>.Default);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = (Namespace != null ? Namespace.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ TargetType.GetHashCode();
            hashCode = (hashCode * 397) ^ Properties.GetHashCode();
            hashCode = (hashCode * 397) ^ Methods.GetHashCode();
            hashCode = (hashCode * 397) ^ (ParentClass != null ? ParentClass.GetHashCode() : 0);
            return hashCode;
        }
    }
}