using System;
using System.Diagnostics;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem;

[DebuggerDisplay("{Kind}", Name = "{Name}")]
public abstract class TypeDefinition : INode, IEquatable<TypeDefinition>
{
    public abstract Directives? Directives { get; }
    public abstract Name Name { get; }

    public bool Equals(TypeDefinition? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name.Equals(other.Name) && Kind == other.Kind;
    }

    public abstract NodeKind Kind { get; }
    public abstract Location? Location { get; }


    public static implicit operator TypeDefinition(string value)
    {
        var parser = Parser.Create(Encoding.UTF8.GetBytes(value));
        return parser.ParseTypeDefinition();
    }

    public static implicit operator TypeDefinition(in ReadOnlySpan<byte> value)
    {
        var parser = Parser.Create(value);
        return parser.ParseTypeDefinition();
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is TypeDefinition other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, (int)Kind);
    }

    public static bool operator ==(TypeDefinition? left, TypeDefinition? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(TypeDefinition? left, TypeDefinition? right)
    {
        return !Equals(left, right);
    }
}