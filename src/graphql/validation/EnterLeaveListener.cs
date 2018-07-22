using System;
using System.Collections.Generic;
using System.Linq;
using GraphQLParser.AST;

namespace fugu.graphql.validation
{
    public class MatchingNodeListener
    {
        public Func<ASTNode, bool> Matches { get; set; }

        public Action<ASTNode> Enter { get; set; }

        public Action<ASTNode> Leave { get; set; }
    }

    public class EnterLeaveListener : INodeVisitor
    {
        private readonly List<MatchingNodeListener> _listeners =
            new List<MatchingNodeListener>();

        public EnterLeaveListener(Action<EnterLeaveListener> configure)
        {
            configure(this);
        }

        public void Leave(ASTNode node)
        {
            foreach (var listener in _listeners.Where(l => l.Leave != null).Where(l => l.Matches(node)))
                listener.Leave(node);
        }

        public void Enter(ASTNode node)
        {
            foreach (var listener in _listeners.Where(l => l.Enter != null).Where(l => l.Matches(node)))
                listener.Enter(node);
        }

        public void Match<T>(
            Action<T> enter = null,
            Action<T> leave = null) where T : ASTNode
        {
            if (enter == null && leave == null)
                throw new InvalidOperationException("Must provide an enter or leave function.");

            bool Matches(ASTNode n)
            {
                return n.GetType().IsAssignableFrom(typeof(T));
            }

            var listener = new MatchingNodeListener
            {
                Matches = Matches
            };

            if (enter != null) listener.Enter = n => enter((T) n);

            if (leave != null) listener.Leave = n => leave((T) n);

            _listeners.Add(listener);
        }
    }
}