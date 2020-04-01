using System;
using Tanka.GraphQL.Language.Nodes;


namespace Tanka.GraphQL.Language
{
    public class DocumentException : Exception
    {
        public DocumentException(
            string message,
            params INode[] nodes): this(message, innerException: null, nodes)
        {
            
        }

        public DocumentException(
            string message,
            Exception? innerException,
            params INode[] nodes): base(message, innerException)
        {
            Nodes = nodes;
        }

        public INode[] Nodes { get; set; }
    }
}