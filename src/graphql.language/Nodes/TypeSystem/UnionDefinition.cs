using System;
using System.Collections.Generic;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class UnionDefinition : TypeDefinition
    {
        public UnionDefinition(
            StringValue? description,
            Name name,
            IReadOnlyCollection<Directive>? directives,
            IReadOnlyCollection<NamedType>? members,
            in Location? location)
        {
            Description = description;
            Name = name;
            Directives = directives;
            Members = members;
            Location = location;
        }

        public StringValue? Description { get; }

        public Name Name { get; }

        public IReadOnlyCollection<Directive>? Directives { get; }

        public IReadOnlyCollection<NamedType>? Members { get; }

        public Location? Location { get; }

        public static implicit operator UnionDefinition(string value)
        {
            var parser = new Parser(Encoding.UTF8.GetBytes(value));
            return parser.ParseUnionDefinition();
        }

        public static implicit operator string(UnionDefinition value)
        {
            throw new NotImplementedException();
        }
    }
}