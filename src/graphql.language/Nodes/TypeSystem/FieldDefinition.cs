using System;
using System.Collections.Generic;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class FieldDefinition: INode
    {
        public NodeKind Kind => NodeKind.FieldDefinition;
        
        public FieldDefinition(StringValue? description,
            in Name name,
            IReadOnlyCollection<InputValueDefinition>? arguments,
            TypeBase type,
            IReadOnlyCollection<Directive>? directives,
            in Location? location = default)
        {
            Description = description;
            Name = name;
            Arguments = arguments;
            Type = type;
            Directives = directives;
            Location = location;
        }

        public StringValue? Description { get; }
        public Name Name { get; }
        public IReadOnlyCollection<InputValueDefinition>? Arguments { get; }
        public TypeBase Type { get; }
        public IReadOnlyCollection<Directive>? Directives { get; }
        public Location? Location { get; }

        public static implicit operator FieldDefinition(string value)
        {
            var parser = Parser.Create(Encoding.UTF8.GetBytes(value));
            return parser.ParseFieldDefinition();
        }

        public static implicit operator FieldDefinition(in ReadOnlySpan<byte> value)
        {
            var parser = Parser.Create(value);
            return parser.ParseFieldDefinition();
        }
    }
}