using System;
using System.Collections.Generic;
using Tanka.GraphQL.Execution;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL
{
    public class QueryExecutionException : DocumentException
    {
        public QueryExecutionException(
            string message,
            NodePath path,
            params INode[] nodes) : this(message, null, path, nodes)
        {
        }

        public QueryExecutionException(
            string message,
            Exception? innerException,
            NodePath path,
            params INode[] nodes) : this(message, innerException, path, null, nodes)
        {
        }

        public QueryExecutionException(
            string message,
            Exception? innerException,
            NodePath path,
            IReadOnlyDictionary<string, object>? extensions,
            params INode[] nodes) : base(message, innerException, nodes)
        {
            Path = path;
            Extensions = extensions;
        }

        public IReadOnlyDictionary<string, object>? Extensions { get; set; }

        public NodePath Path { get; set; }
    }
}