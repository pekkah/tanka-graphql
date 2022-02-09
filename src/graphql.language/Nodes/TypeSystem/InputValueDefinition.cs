using System;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class InputValueDefinition : INode
    {
        public InputValueDefinition(
            StringValue? description,
            in Name name,
            TypeBase type,
            DefaultValue? defaultValue,
            Directives? directives,
            in Location? location = default)
        {
            Description = description;
            Name = name;
            Type = type;
            DefaultValue = defaultValue;
            Directives = directives;
            Location = location;
        }

        public DefaultValue? DefaultValue { get; }

        public StringValue? Description { get; }

        public Directives? Directives { get; }

        public Name Name { get; }

        public TypeBase Type { get; }
        public NodeKind Kind => NodeKind.InputValueDefinition;

        public Location? Location { get; }

        public static implicit operator InputValueDefinition(string value)
        {
            var parser = Parser.Create(Encoding.UTF8.GetBytes(value));
            return parser.ParseInputValueDefinition();
        }

        public static implicit operator InputValueDefinition(in ReadOnlySpan<byte> value)
        {
            var parser = Parser.Create(value);
            return parser.ParseInputValueDefinition();
        }
    }
}