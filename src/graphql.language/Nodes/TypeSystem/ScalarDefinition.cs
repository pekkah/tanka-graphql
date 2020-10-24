using System;
using System.Collections.Generic;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class ScalarDefinition : TypeDefinition
    {
        public override NodeKind Kind => NodeKind.ScalarDefinition;
        public ScalarDefinition(
            StringValue? description,
            in Name name,
            Directives? directives,
            in Location? location = default)
        {
            Description = description;
            Name = name;
            Directives = directives;
            Location = location;
        }

        public StringValue? Description { get; }
        public override Name Name { get; }
        public Directives? Directives { get; }
        public override Location? Location { get; }

        public static implicit operator ScalarDefinition(string value)
        {
            var parser = Parser.Create(Encoding.UTF8.GetBytes(value));
            return parser.ParseScalarDefinition();
        }

        public static implicit operator ScalarDefinition(in ReadOnlySpan<byte> value)
        {
            var parser = Parser.Create(value);
            return parser.ParseScalarDefinition();
        }
    }
}