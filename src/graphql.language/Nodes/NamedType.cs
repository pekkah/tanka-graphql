using System;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class NamedType : TypeBase
    {
        public override NodeKind Kind => NodeKind.NamedType;
        public override Location? Location {get;}
        public readonly Name Name;

        public NamedType(
            in Name name,
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
    }
}