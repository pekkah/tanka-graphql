using System;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class NamedType : Type
    {
        public readonly Location? Location;
        public readonly Name Name;

        public NamedType(
            Name name,
            in Location? location = default)
        {
            Name = name;
            Location = location;
        }

        public static implicit operator NamedType(string value)
        {
            var parser = new Parser(Encoding.UTF8.GetBytes(value));
            return parser.ParseNamedType();
        }

        public static implicit operator string(NamedType value)
        {
            throw new NotImplementedException();
        }
    }
}