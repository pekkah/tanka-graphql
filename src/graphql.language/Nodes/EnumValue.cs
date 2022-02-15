using System;
using System.Diagnostics;

namespace Tanka.GraphQL.Language.Nodes;

[DebuggerDisplay("{Name}")]
public sealed class EnumValue : ValueBase, INode, IEquatable<EnumValue>, IEquatable<string>
{
    public readonly Name Name;

    public EnumValue(
        in Name name,
        in Location? location = default)
    {
        Name = name;
        Location = location;
    }

    public bool Equals(EnumValue? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name.Equals(other.Name);
    }

    public bool Equals(string? other)
    {
        return Name.Equals(other);
    }

    public override NodeKind Kind => NodeKind.EnumValue;
    public override Location? Location { get; }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    public static bool operator ==(EnumValue? left, EnumValue? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(EnumValue? left, EnumValue? right)
    {
        return !Equals(left, right);
    }

    public bool Equals(Enum systemEnum)
    {
        var enumName = Enum.GetName(systemEnum.GetType(), systemEnum);

        return Equals(enumName);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
            return true;

        if (obj is EnumValue otherValue && Equals(otherValue))
            return true;

        if (obj is string otherStr && Equals(otherStr))
            return true;

        if (obj is Enum otherEnum && Equals(otherEnum))
            return true;

        return false;
    }
}