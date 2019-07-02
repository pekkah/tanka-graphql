using System;
using GraphQLParser.AST;

namespace tanka.graphql.language
{
    public class DocumentException : Exception
    {
        public DocumentException(
            string message,
            params ASTNode[] nodes): this(message, innerException: null, nodes)
        {
            
        }

        public DocumentException(
            string message,
            Exception innerException,
            params ASTNode[] nodes): base(message, innerException)
        {
            Nodes = nodes;
        }

        public ASTNode[] Nodes { get; set; }
    }
}