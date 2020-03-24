using System;
using System.Collections.Generic;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class DirectiveDefinition
    {
        public DirectiveDefinition(
            StringValue? description,
            Name name,
            IReadOnlyCollection<InputValueDefinition>? arguments,
            in bool isRepeatable,
            IReadOnlyCollection<string> directiveLocations,
            in Location? location)
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
        public IReadOnlyCollection<InputValueDefinition>? Arguments { get; }
        public bool IsRepeatable { get; }
        public IReadOnlyCollection<string> DirectiveLocations { get; }
        public Location? Location { get; }

        public static implicit operator DirectiveDefinition(string value)
        {
            var parser = new Parser(Encoding.UTF8.GetBytes(value));
            return parser.ParseDirectiveDefinition();
        }

        public static implicit operator string(DirectiveDefinition value)
        {
            throw new NotImplementedException();
        }
    }
}