using System;
using System.Collections.Generic;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class ScalarDefinition : TypeDefinition
    {
        public ScalarDefinition(
            StringValue? description,
            Name name,
            IReadOnlyCollection<Directive>? directives,
            in Location? location = default)
        {
            Description = description;
            Name = name;
            Directives = directives;
            Location = location;
        }

        public StringValue? Description { get; }
        public Name Name { get; }
        public IReadOnlyCollection<Directive>? Directives { get; }
        public Location? Location { get; }

        public static implicit operator ScalarDefinition(string value)
        {
            var parser = new Parser(Encoding.UTF8.GetBytes(value));
            return parser.ParseScalarDefinition();
        }

        public static implicit operator string(ScalarDefinition value)
        {
            throw new NotImplementedException();
        }
    }
}