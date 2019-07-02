using System;
using System.Collections.Generic;
using GraphQLParser.AST;
using tanka.graphql.execution;

namespace tanka.graphql.resolvers
{
    public class CompleteValueException : QueryExecutionException
    {
        public CompleteValueException(string message, NodePath path, params ASTNode[] nodes) : base(message, path,
            nodes)
        {
        }

        public CompleteValueException(string message, Exception innerException, NodePath path, params ASTNode[] nodes) :
            base(message, innerException, path, nodes)
        {
        }

        public CompleteValueException(string message, Exception innerException, NodePath path,
            IReadOnlyDictionary<string, object> extensions, params ASTNode[] nodes) : base(message, innerException,
            path, extensions, nodes)
        {
        }
    }
}