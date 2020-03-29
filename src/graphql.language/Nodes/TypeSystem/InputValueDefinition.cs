using System;
using System.Collections.Generic;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class InputValueDefinition
    {
        public InputValueDefinition(
            StringValue? description,
            in Name name,
            Type type,
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

        public Type Type { get; }

        public DefaultValue? DefaultValue { get; }

        public IReadOnlyCollection<Directive>? Directives { get; }

        public Location? Location { get; }

        public static implicit operator InputValueDefinition(string value)
        {
            var parser = new Parser(Encoding.UTF8.GetBytes(value));
            return parser.ParseInputValueDefinition();
        }

        public static implicit operator string(InputValueDefinition value)
        {
            throw new NotImplementedException();
        }
    }
}