using System;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes
{
    public abstract class Type
    {
        public static implicit operator Type(string value)
        {
            var parser = new Parser(Encoding.UTF8.GetBytes(value));
            return parser.ParseType();
        }

        public static implicit operator string(Type value)
        {
            throw new NotImplementedException();
        }
    }
}