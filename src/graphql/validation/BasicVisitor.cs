using System.Collections.Generic;
using System.Linq;
using GraphQLParser;
using GraphQLParser.AST;

namespace tanka.graphql.validation
{
    public class BasicVisitor : GraphQLAstVisitor
    {
        private readonly IEnumerable<INodeVisitor> _visitors;

        public BasicVisitor(params INodeVisitor[] visitors)
        {
            _visitors = visitors;
        }

        public override void Visit(GraphQLDocument ast)
        {
            //foreach (var visitor in _visitors) visitor.Enter(ast);
            base.Visit(ast);
            //foreach (var visitor in _visitors.Reverse()) visitor.Leave(ast);
        }

        public override ASTNode BeginVisitNode(ASTNode node)
        {
            foreach (var visitor in _visitors) visitor.Enter(node);

            var result = base.BeginVisitNode(node);

            foreach (var visitor in _visitors.Reverse()) visitor.Leave(node);

            return result;
        }
    }
}