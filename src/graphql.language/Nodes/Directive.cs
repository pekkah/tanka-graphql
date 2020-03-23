using System;
using System.Collections.Generic;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class Directive
    {
        public readonly IReadOnlyCollection<Argument>? Arguments;
        public readonly Location? Location;
        public readonly Name Name;

        public Directive(
            Name name,
            IReadOnlyCollection<Argument>? arguments,
            in Location? location)
        {
            Name = name;
            Arguments = arguments;
            Location = location;
        }

        public static implicit operator Directive(string value)
        {
            var parser = new Parser(Encoding.UTF8.GetBytes(value));
            return parser.ParseDirective(true);
        }

        public static implicit operator string(Directive value)
        {
            throw new NotImplementedException();
        }
    }
}