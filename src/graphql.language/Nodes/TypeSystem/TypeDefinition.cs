using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public abstract class TypeDefinition: INode
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
        
        public abstract NodeKind Kind { get; }
        public abstract Location? Location { get; }
        public abstract Name Name { get; }
    }
}