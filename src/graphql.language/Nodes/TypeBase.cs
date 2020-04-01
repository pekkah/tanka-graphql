using System;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes
{
    public abstract class TypeBase: INode
    {
        public abstract Location? Location { get; }
        public abstract NodeKind Kind { get; }
        
        public static implicit operator TypeBase(string value)
        {
            var parser = Parser.Create(Encoding.UTF8.GetBytes(value));
            return parser.ParseType();
        }

        public static implicit operator TypeBase(in ReadOnlySpan<byte> value)
        {
            var parser = Parser.Create(value);
            return parser.ParseType();
        }
    }
}