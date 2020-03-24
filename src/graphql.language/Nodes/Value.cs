using System;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes
{
    public abstract class Value
    {
        public static implicit operator Value(string value)
        {
            var parser = new Parser(Encoding.UTF8.GetBytes(value));
            return parser.ParseValue(true);
        }

        public static implicit operator string(Value value)
        {
            throw new NotImplementedException();
        }
    }
}