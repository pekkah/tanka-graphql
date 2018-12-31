using System;
using System.Collections.Generic;
using tanka.graphql.error;
using tanka.graphql.execution;
using GraphQLParser;
using GraphQLParser.AST;

namespace tanka.graphql.resolvers
{
    public class CompleteValueException : GraphQLError
    {
        public CompleteValueException(string message) : base(message)
        {
        }

        public CompleteValueException(string message, params ASTNode[] nodes) : base(message, nodes)
        {
        }

        public CompleteValueException(string message, IEnumerable<ASTNode> nodes, ISource source = null, IEnumerable<GraphQLLocation> locations = null, NodePath path = null, Dictionary<string, object> extensions = null, Exception originalError = null) : base(message, nodes, source, locations, path, extensions, originalError)
        {
        }
    }
}