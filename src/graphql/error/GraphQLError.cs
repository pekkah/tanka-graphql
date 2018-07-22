using System;
using System.Collections.Generic;
using System.Linq;
using GraphQLParser;
using GraphQLParser.AST;

namespace fugu.graphql.error
{
    public class GraphQLError : Exception
    {
        public GraphQLError(string message) : base(message)
        {
        }

        public GraphQLError(string message, params ASTNode[] nodes) : this(message)
        {
            if (nodes != null)
                Nodes.AddRange(nodes);
        }

        public GraphQLError(
            string message,
            IEnumerable<ASTNode> nodes,
            ISource source,
            IEnumerable<int> positions,
            IEnumerable<string> paths /*,
            originalError?: ?Error,
            extensions?: ?{ [key: string]: mixed },*/
        ) : base(message)
        {
            Nodes = nodes?.ToList();
            GQLSource = source;
            Positions = positions?.ToList();
            Paths = paths?.ToList();
        }

        public List<string> Paths { get; set; }

        public List<int> Positions { get; set; }

        public ISource GQLSource { get; set; }

        public List<ASTNode> Nodes { get; set; } = new List<ASTNode>();
    }
}