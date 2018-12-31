using System;
using System.Collections.Generic;
using System.Linq;
using tanka.graphql.execution;
using GraphQLParser;
using GraphQLParser.AST;

namespace tanka.graphql.error
{
    public class GraphQLError : Exception
    {
        public GraphQLError(string message) : base(message)
        {
        }

        public GraphQLError(string message, params ASTNode[] nodes) : this(message)
        {
            Nodes = nodes?.ToList();
        }

        public GraphQLError(string message,
            IEnumerable<ASTNode> nodes,
            ISource source = null,
            IEnumerable<GraphQLLocation> locations = null,
            NodePath path = null,
            Dictionary<string,object> extensions = null,
            Exception originalError = null) : base(message, originalError)
        {
            Nodes = nodes?.ToList();
            GQLSource = source;
            Locations = locations?.ToList();
            Path = path;
            Extensions = extensions;
        }

        public Dictionary<string, object> Extensions { get; set; }

        public NodePath Path { get; set; }

        public List<GraphQLLocation> Locations { get; set; }

        public ISource GQLSource { get; set; }

        public List<ASTNode> Nodes { get; set; }
    }
}