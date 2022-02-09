using System;
using System.Diagnostics;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    [DebuggerDisplay("{Value}")]
    public sealed class EnumValueDefinition : INode
    {
        public EnumValueDefinition(
            StringValue? description,
            EnumValue value,
            Directives? directives,
            in Location? location = default)
        {
            Description = description;
            Value = value;
            Directives = directives;
            Location = location;
        }

        public StringValue? Description { get; }
        public EnumValue Value { get; }
        public Directives? Directives { get; }
        public NodeKind Kind => NodeKind.EnumValueDefinition;
        public Location? Location { get; }

        public static implicit operator EnumValueDefinition(string value)
        {
            var parser = Parser.Create(Encoding.UTF8.GetBytes(value));
            return parser.ParseEnumValueDefinition();
        }

        public static implicit operator EnumValueDefinition(in ReadOnlySpan<byte> value)
        {
            var parser = Parser.Create(value);
            return parser.ParseEnumValueDefinition();
        }
    }
}