using System.Collections.Generic;
using System.Linq;
using GraphQLParser.AST;
using tanka.graphql.language;
using tanka.graphql.type;

namespace tanka.graphql.validation
{
    public class RulesWalker : Visitor, IRuleVisitorContext
    {
        private readonly List<ValidationError> _errors =
            new List<ValidationError>();

        private readonly Dictionary<GraphQLOperationDefinition, List<GraphQLFragmentDefinition>> _fragments =
            new Dictionary<GraphQLOperationDefinition, List<GraphQLFragmentDefinition>>();


        private readonly Dictionary<GraphQLOperationDefinition, List<VariableUsage>> _variables
            = new Dictionary<GraphQLOperationDefinition, List<VariableUsage>>();

        public RulesWalker(
            IEnumerable<CombineRule> rules,
            ISchema schema,
            GraphQLDocument document,
            IReadOnlyDictionary<string, object> variableValues = null)
        {
            Schema = schema;
            Document = document;
            VariableValues = variableValues;
            CreateVisitors(rules);
        }

        public GraphQLDocument Document { get; }

        public IReadOnlyDictionary<string, object> VariableValues { get; }

        public TypeTracker Tracker { get; protected set; }

        public ExtensionData Extensions { get; } = new ExtensionData();

        public ISchema Schema { get; }

        public void Error(string code, string message, params ASTNode[] nodes)
        {
            _errors.Add(new ValidationError(code, message, nodes));
        }

        public void Error(string code, string message, ASTNode node)
        {
            _errors.Add(new ValidationError(code, message, node));
        }

        public void Error(string code, string message, IEnumerable<ASTNode> nodes)
        {
            _errors.Add(new ValidationError(code, message, nodes));
        }

        public List<VariableUsage> GetVariables(
            ASTNode rootNode)
        {
            var usages = new List<VariableUsage>();

            var visitor = new RulesWalker(new CombineRule[]
                {
                    (context, rule) =>
                    {
                        rule.EnterVariable += node =>
                        {
                            usages.Add(new VariableUsage
                            {
                                Node = node,
                                Type = context.Tracker.GetInputType(),
                                DefaultValue = context.Tracker.GetDefaultValue()
                            });
                        };
                    }
                },
                Schema,
                Document,
                VariableValues);


            visitor.BeginVisitNode(rootNode);

            return usages;
        }

        public IEnumerable<VariableUsage> GetRecursiveVariables(
            GraphQLOperationDefinition operation)
        {
            if (_variables.TryGetValue(operation, out var results)) return results;

            var usages = GetVariables(operation);

            foreach (var fragment in GetRecursivelyReferencedFragments(operation))
                usages.AddRange(GetVariables(fragment));

            _variables[operation] = usages;

            return usages;
        }

        public GraphQLFragmentDefinition GetFragment(string name)
        {
            return Document.Definitions.OfType<GraphQLFragmentDefinition>()
                .SingleOrDefault(f => f.Name.Value == name);
        }

        public List<GraphQLFragmentSpread> GetFragmentSpreads(GraphQLSelectionSet node)
        {
            var spreads = new List<GraphQLFragmentSpread>();

            var setsToVisit = new Stack<GraphQLSelectionSet>(new[] {node});

            while (setsToVisit.Count > 0)
            {
                var set = setsToVisit.Pop();

                foreach (var selection in set.Selections)
                    switch (selection)
                    {
                        case GraphQLFragmentSpread spread:
                            spreads.Add(spread);
                            break;
                        case GraphQLInlineFragment inlineFragment:
                        {
                            if (inlineFragment.SelectionSet != null)
                                setsToVisit.Push(inlineFragment.SelectionSet);
                            break;
                        }

                        case GraphQLOperationDefinition operationDefinition:
                        {
                            if (operationDefinition.SelectionSet != null)
                                setsToVisit.Push(operationDefinition.SelectionSet);
                            break;
                        }

                        case GraphQLFieldSelection fieldSelection:
                        {
                            if (fieldSelection.SelectionSet != null) setsToVisit.Push(fieldSelection.SelectionSet);
                            break;
                        }
                    }
            }

            return spreads;
        }

        public IEnumerable<GraphQLFragmentDefinition> GetRecursivelyReferencedFragments(
            GraphQLOperationDefinition operation)
        {
            if (_fragments.TryGetValue(operation, out var results)) return results;

            var fragments = new List<GraphQLFragmentDefinition>();
            var nodesToVisit = new Stack<GraphQLSelectionSet>(new[]
            {
                operation.SelectionSet
            });
            var collectedNames = new Dictionary<string, bool>();

            while (nodesToVisit.Count > 0)
            {
                var node = nodesToVisit.Pop();

                foreach (var spread in GetFragmentSpreads(node))
                {
                    var fragName = spread.Name.Value;
                    if (!collectedNames.ContainsKey(fragName))
                    {
                        collectedNames[fragName] = true;

                        var fragment = GetFragment(fragName);
                        if (fragment != null)
                        {
                            fragments.Add(fragment);
                            nodesToVisit.Push(fragment.SelectionSet);
                        }
                    }
                }
            }

            _fragments[operation] = fragments;

            return fragments;
        }

        public ValidationResult Validate()
        {
            Visit(Document);
            return BuildResult();
        }

        public override void Visit(GraphQLDocument document)
        {
            Tracker.EnterDocument?.Invoke(document);

            base.Visit(document);

            Tracker.LeaveDocument?.Invoke(document);
        }

        public override GraphQLName BeginVisitAlias(GraphQLName alias)
        {
            {
                Tracker.EnterAlias?.Invoke(alias);
            }

            return base.BeginVisitAlias(alias);
        }

        public override GraphQLArgument BeginVisitArgument(GraphQLArgument argument)
        {
            {
                Tracker.EnterArgument?.Invoke(argument);
            }

            return base.BeginVisitArgument(argument);
        }

        public override GraphQLScalarValue BeginVisitBooleanValue(
            GraphQLScalarValue value)
        {
            {
                Tracker.EnterBooleanValue?.Invoke(value);
            }

            return base.BeginVisitBooleanValue(value);
        }

        public override GraphQLDirective BeginVisitDirective(GraphQLDirective directive)
        {
            Tracker.EnterDirective?.Invoke(directive);

            var _ = base.BeginVisitDirective(directive);

            Tracker.LeaveDirective?.Invoke(_);
            return _;
        }

        public override GraphQLScalarValue BeginVisitEnumValue(GraphQLScalarValue value)
        {
            {
                Tracker.EnterEnumValue?.Invoke(value);
            }

            var _ = base.BeginVisitEnumValue(value);


            {
                Tracker.LeaveEnumValue?.Invoke(value);
            }

            return _;
        }

        public override GraphQLFieldSelection BeginVisitFieldSelection(
            GraphQLFieldSelection selection)
        {
            Tracker.EnterFieldSelection?.Invoke(selection);

            return base.BeginVisitFieldSelection(selection);
        }

        public override GraphQLScalarValue BeginVisitFloatValue(
            GraphQLScalarValue value)
        {
            {
                Tracker.EnterFloatValue?.Invoke(value);
            }

            return base.BeginVisitFloatValue(value);
        }

        public override GraphQLFragmentDefinition BeginVisitFragmentDefinition(
            GraphQLFragmentDefinition node)
        {
            {
                Tracker.EnterFragmentDefinition?.Invoke(node);
            }

            var result = base.BeginVisitFragmentDefinition(node);


            {
                Tracker.LeaveFragmentDefinition?.Invoke(node);
            }

            return result;
        }

        public override GraphQLFragmentSpread BeginVisitFragmentSpread(
            GraphQLFragmentSpread fragmentSpread)
        {
            {
                Tracker.EnterFragmentSpread?.Invoke(fragmentSpread);
            }

            return base.BeginVisitFragmentSpread(fragmentSpread);
        }

        public override GraphQLInlineFragment BeginVisitInlineFragment(
            GraphQLInlineFragment inlineFragment)
        {
            {
                Tracker.EnterInlineFragment?.Invoke(inlineFragment);
            }

            var _ = base.BeginVisitInlineFragment(inlineFragment);


            {
                Tracker.LeaveInlineFragment?.Invoke(inlineFragment);
            }

            return _;
        }

        public override GraphQLScalarValue BeginVisitIntValue(GraphQLScalarValue value)
        {
            {
                Tracker.EnterIntValue?.Invoke(value);
            }

            return base.BeginVisitIntValue(value);
        }

        public override GraphQLName BeginVisitName(GraphQLName name)
        {
            {
                Tracker.EnterName?.Invoke(name);
            }

            return base.BeginVisitName(name);
        }

        public override GraphQLNamedType BeginVisitNamedType(
            GraphQLNamedType typeCondition)
        {
            {
                Tracker.EnterNamedType?.Invoke(typeCondition);
            }

            return base.BeginVisitNamedType(typeCondition);
        }

        public override GraphQLOperationDefinition BeginVisitOperationDefinition(
            GraphQLOperationDefinition definition)
        {
            {
                Tracker.EnterOperationDefinition?.Invoke(definition);
            }

            return base.BeginVisitOperationDefinition(definition);
        }

        public override GraphQLOperationDefinition EndVisitOperationDefinition(
            GraphQLOperationDefinition definition)
        {
            {
                Tracker.LeaveOperationDefinition?.Invoke(definition);
            }

            return base.EndVisitOperationDefinition(definition);
        }

        public override GraphQLSelectionSet BeginVisitSelectionSet(
            GraphQLSelectionSet selectionSet)
        {
            {
                Tracker.EnterSelectionSet?.Invoke(selectionSet);
            }

            var _ = base.BeginVisitSelectionSet(selectionSet);


            {
                Tracker.LeaveSelectionSet?.Invoke(selectionSet);
            }

            return _;
        }

        public override GraphQLScalarValue BeginVisitStringValue(
            GraphQLScalarValue value)
        {
            {
                Tracker.EnterStringValue?.Invoke(value);
            }

            return base.BeginVisitStringValue(value);
        }

        public override GraphQLVariable BeginVisitVariable(GraphQLVariable variable)
        {
            Tracker.EnterVariable?.Invoke(variable);
            return base.BeginVisitVariable(variable);
        }

        public override GraphQLVariableDefinition BeginVisitVariableDefinition(
            GraphQLVariableDefinition node)
        {
            Tracker.EnterVariableDefinition?.Invoke(node);

            var _ = base.BeginVisitVariableDefinition(node);

            Tracker.LeaveVariableDefinition?.Invoke(node);
            return _;
        }

        public override GraphQLArgument EndVisitArgument(GraphQLArgument argument)
        {
            {
                Tracker.LeaveArgument?.Invoke(argument);
            }

            return base.EndVisitArgument(argument);
        }

        public override GraphQLFieldSelection EndVisitFieldSelection(
            GraphQLFieldSelection selection)
        {
            {
                Tracker.LeaveFieldSelection?.Invoke(selection);
            }

            return base.EndVisitFieldSelection(selection);
        }

        public override GraphQLVariable EndVisitVariable(GraphQLVariable variable)
        {
            Tracker.LeaveVariable?.Invoke(variable);
            return base.EndVisitVariable(variable);
        }

        public override GraphQLObjectField BeginVisitObjectField(
            GraphQLObjectField node)
        {
            {
                Tracker.EnterObjectField?.Invoke(node);
            }

            var _ = base.BeginVisitObjectField(node);


            {
                Tracker.LeaveObjectField?.Invoke(node);
            }

            return _;
        }

        public override GraphQLObjectValue BeginVisitObjectValue(
            GraphQLObjectValue node)
        {
            {
                Tracker.EnterObjectValue?.Invoke(node);
            }

            return base.BeginVisitObjectValue(node);
        }

        public override GraphQLObjectValue EndVisitObjectValue(GraphQLObjectValue node)
        {
            {
                Tracker.LeaveObjectValue?.Invoke(node);
            }

            return base.EndVisitObjectValue(node);
        }

        public override ASTNode BeginVisitNode(ASTNode node)
        {
            Tracker.EnterNode?.Invoke(node);
            return base.BeginVisitNode(node);
        }

        public override GraphQLListValue BeginVisitListValue(GraphQLListValue node)
        {
            {
                Tracker.EnterListValue?.Invoke(node);
            }

            return base.BeginVisitListValue(node);
        }

        public override GraphQLListValue EndVisitListValue(GraphQLListValue node)
        {
            {
                Tracker.LeaveListValue?.Invoke(node);
            }

            return base.EndVisitListValue(node);
        }

        protected void CreateVisitors(IEnumerable<CombineRule> rules)
        {
            Tracker = new TypeTracker(Schema);

            foreach (var createRule in rules) createRule(this, Tracker);
        }

        private ValidationResult BuildResult()
        {
            return new ValidationResult
            {
                Errors = _errors,
                Extensions = Extensions.Data
            };
        }
    }
}