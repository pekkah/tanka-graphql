using System;
using System.Collections.Generic;
using GraphQLParser.AST;
using Tanka.GraphQL.Execution;
using Tanka.GraphQL.Language;

namespace Tanka.GraphQL
{
    public class QueryExecutionException : DocumentException
    {
        public QueryExecutionException(
            string message,
            NodePath path,
            params ASTNode[] nodes) : this(message, null, path, nodes)
        {
        }

        public QueryExecutionException(
            string message,
            Exception innerException,
            NodePath path,
            params ASTNode[] nodes) : this(message, innerException, path, null, nodes)
        {
        }

        public QueryExecutionException(
            string message,
            Exception innerException,
            NodePath path,
            IReadOnlyDictionary<string, object> extensions,
            params ASTNode[] nodes) : base(message, innerException, nodes)
        {
            Path = path;
            Extensions = extensions;
        }

        public IReadOnlyDictionary<string, object> Extensions { get; set; }

        public NodePath Path { get; set; }
    }
}