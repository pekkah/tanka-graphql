using System;
using System.Collections.Generic;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class InputValueDefinition: INode
    {
        public NodeKind Kind => NodeKind.InputValueDefinition;
        
        public InputValueDefinition(
            StringValue? description,
            in Name name,
            TypeBase type,
            DefaultValue? defaultValue,
            IReadOnlyCollection<Directive>? directives,
            in Location? location = default)
        {
            Description = description;
            Name = name;
            Type = type;
            DefaultValue = defaultValue;
            Directives = directives;
            Location = location;
        }

        public StringValue? Description { get; }

        public Name Name { get; }

        public TypeBase Type { get; }

        public DefaultValue? DefaultValue { get; }

        public IReadOnlyCollection<Directive>? Directives { get; }

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