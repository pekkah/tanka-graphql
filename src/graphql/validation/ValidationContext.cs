using System;
using System.Collections.Generic;
using System.Linq;
using tanka.graphql.type;
using GraphQLParser.AST;

namespace tanka.graphql.validation
{
    public class ValidationContext
    {
        private readonly List<ValidationError> _errors = new List<ValidationError>();

        private readonly Dictionary<GraphQLOperationDefinition, IEnumerable<GraphQLFragmentDefinition>> _fragments
            = new Dictionary<GraphQLOperationDefinition, IEnumerable<GraphQLFragmentDefinition>>();

        private readonly Dictionary<GraphQLOperationDefinition, IEnumerable<VariableUsage>> _variables =
            new Dictionary<GraphQLOperationDefinition, IEnumerable<VariableUsage>>();

        public ISchema Schema { get; set; }

        public GraphQLDocument Document { get; set; }

        public TypeInfo TypeInfo { get; set; }

        public IEnumerable<ValidationError> Errors => _errors;
        
        public IDictionary<string, object> Variables { get; set; }

        public void ReportError(ValidationError error)
        {
            if (error == null) throw new ArgumentNullException(nameof(error));

            _errors.Add(error);
        }

        public IEnumerable<VariableUsage> GetVariables(ASTNode node)
        {
            var usages = new List<VariableUsage>();
            var info = new TypeInfo(Schema);

            var listener = new EnterLeaveListener(_ =>
            {
                _.Match<GraphQLVariable>(
                    varRef => usages.Add(new VariableUsage {Node = varRef, Type = info.GetInputType()})
                );
            });

            var visitor = new BasicVisitor(info, listener);
            visitor.BeginVisitNode(node);

            return usages;
        }

        public IEnumerable<VariableUsage> GetRecursiveVariables(GraphQLOperationDefinition graphQLOperationDefinition)
        {
            if (_variables.TryGetValue(graphQLOperationDefinition, out var results))
            {
                return results;
            }

            var usages = GetVariables(graphQLOperationDefinition).ToList();
            var fragments = GetRecursivelyReferencedFragments(graphQLOperationDefinition);

            foreach (var fragment in fragments)
            {
                usages.AddRange(GetVariables(fragment));
            }

            _variables[graphQLOperationDefinition] = usages;

            return usages;
        }

        public GraphQLFragmentDefinition GetFragment(string name)
        {
            return Document.Definitions.OfType<GraphQLFragmentDefinition>().SingleOrDefault(f => f.Name.Value == name);
        }

        public IEnumerable<GraphQLFragmentSpread> GetFragmentSpreads(GraphQLSelectionSet node)
        {
            var spreads = new List<GraphQLFragmentSpread>();

            var setsToVisit = new Stack<GraphQLSelectionSet>(new[] {node});

            while (setsToVisit.Any())
            {
                var set = setsToVisit.Pop();

                foreach (var selection in set.Selections)
                {
                    if (selection is GraphQLFragmentSpread spread)
                    {
                        spreads.Add(spread);
                    }
                    else
                    {
                        if (selection is GraphQLSelectionSet hasSet)
                        {
                            setsToVisit.Push(hasSet);
                        }
                    }
                }
            }

            return spreads;
        }

        public IEnumerable<GraphQLFragmentDefinition> GetRecursivelyReferencedFragments(GraphQLOperationDefinition graphQLOperationDefinition)
        {
            if (_fragments.TryGetValue(graphQLOperationDefinition, out var results))
            {
                return results;
            }

            var fragments = new List<GraphQLFragmentDefinition>();
            var nodesToVisit = new Stack<GraphQLSelectionSet>(new[]
            {
                graphQLOperationDefinition.SelectionSet
            });

            var collectedNames = new Dictionary<string, bool>();

            while (nodesToVisit.Any())
            {
                var node = nodesToVisit.Pop();
                var spreads = GetFragmentSpreads(node);

                foreach (var spread in spreads)
                {
                    var fragName = spread.Name.Value;
                    if (collectedNames.ContainsKey(fragName)) 
                        continue;
                    
                    collectedNames[fragName] = true;

                    var fragment = GetFragment(fragName);
                    if (fragment != null)
                    {
                        fragments.Add(fragment);
                        nodesToVisit.Push(fragment.SelectionSet);
                    }
                }
            }

            _fragments[graphQLOperationDefinition] = fragments;

            return fragments;
        }

    }
}
