using System;
using System.Text;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Language
{
    public class Printer
    {
        private StringBuilder _builder = new StringBuilder();
        
        public string Print(INode node)
        {
            PrintNode(node);
            return _builder.ToString();
        }

        private void PrintNode(INode node)
        {
        }
    }
}