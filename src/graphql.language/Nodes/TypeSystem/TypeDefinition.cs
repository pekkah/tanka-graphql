using System;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public abstract class TypeDefinition
    {
        public static implicit operator TypeDefinition(string value)
        {
            var parser = Parser.Create(Encoding.UTF8.GetBytes(value));
            return parser.ParseTypeDefinition();
        }

        public static implicit operator TypeDefinition(in ReadOnlySpan<byte> value)
        {
            var parser = Parser.Create(value);
            return parser.ParseTypeDefinition();
        }

        public static implicit operator string(TypeDefinition value)
        {
            throw new NotImplementedException();
        }
    }
}