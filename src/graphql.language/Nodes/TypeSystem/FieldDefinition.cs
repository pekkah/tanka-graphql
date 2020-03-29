using System;
using System.Collections.Generic;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class FieldDefinition
    {
        public FieldDefinition(StringValue? description,
            in Name name,
            IReadOnlyCollection<InputValueDefinition>? arguments,
            Type type,
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
        public Type Type { get; }
        public IReadOnlyCollection<Directive>? Directives { get; }
        public Location? Location { get; }

        public static implicit operator FieldDefinition(string value)
        {
            var parser = new Parser(Encoding.UTF8.GetBytes(value));
            return parser.ParseFieldDefinition();
        }

        public static implicit operator string(FieldDefinition value)
        {
            throw new NotImplementedException();
        }
    }
}