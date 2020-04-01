using System;
using System.Collections.Generic;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class UnionDefinition : TypeDefinition
    {
        public override NodeKind Kind => NodeKind.UnionDefinition;
        public UnionDefinition(
            StringValue? description,
            in Name name,
            IReadOnlyCollection<Directive>? directives,
            IReadOnlyCollection<NamedType>? members,
            in Location? location = default)
        {
            Description = description;
            Name = name;
            Directives = directives;
            Members = members;
            Location = location;
        }

        public StringValue? Description { get; }

        public override Name Name { get; }

        public IReadOnlyCollection<Directive>? Directives { get; }

        public IReadOnlyCollection<NamedType>? Members { get; }

        public override Location? Location { get; }

        public static implicit operator UnionDefinition(string value)
        {
            var parser = Parser.Create(Encoding.UTF8.GetBytes(value));
            return parser.ParseUnionDefinition();
        }

        public static implicit operator UnionDefinition(in ReadOnlySpan<byte> value)
        {
            var parser = Parser.Create(value);
            return parser.ParseUnionDefinition();
        }
    }
}