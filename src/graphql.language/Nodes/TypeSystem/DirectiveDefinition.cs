using System;
using System.Collections.Generic;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class DirectiveDefinition : INode
    {
        public DirectiveDefinition(
            StringValue? description,
            in Name name,
            ArgumentsDefinition? arguments,
            in bool isRepeatable,
            IReadOnlyList<string> directiveLocations,
            in Location? location = default)
        {
            Description = description;
            Name = name;
            Arguments = arguments;
            IsRepeatable = isRepeatable;
            DirectiveLocations = directiveLocations;
            Location = location;
        }

        public StringValue? Description { get; }
        public Name Name { get; }
        public ArgumentsDefinition? Arguments { get; }
        public bool IsRepeatable { get; }
        public IReadOnlyList<string> DirectiveLocations { get; }
        public NodeKind Kind => NodeKind.DirectiveDefinition;
        public Location? Location { get; }

        public static implicit operator DirectiveDefinition(string value)
        {
            var parser = new Parser(Encoding.UTF8.GetBytes(value));
            return parser.ParseDirectiveDefinition();
        }

        public static implicit operator DirectiveDefinition(in ReadOnlySpan<byte> value)
        {
            var parser = new Parser(value);
            return parser.ParseDirectiveDefinition();
        }
    }
}