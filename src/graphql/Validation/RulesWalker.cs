using System.Collections.Generic;
using System.Linq;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.TypeSystem;
using Argument = Tanka.GraphQL.Language.Nodes.Argument;
using EnumValue = Tanka.GraphQL.Language.Nodes.EnumValue;

namespace Tanka.GraphQL.Validation
{
    public class RulesWalker : Visitor, IRuleVisitorContext
    {
        private readonly List<ValidationError> _errors =
            new List<ValidationError>();

        private readonly Dictionary<OperationDefinition, List<FragmentDefinition>> _fragments =
            new Dictionary<OperationDefinition, List<FragmentDefinition>>();


        private readonly Dictionary<OperationDefinition, List<VariableUsage>> _variables
            = new Dictionary<OperationDefinition, List<VariableUsage>>();

        public RulesWalker(
            IEnumerable<CombineRule> rules,
            ISchema schema,
            ExecutableDocument document,
            IReadOnlyDictionary<string, object?>? variableValues = null)
        {
            Schema = schema;
            Document = document;
            VariableValues = variableValues;
            CreateVisitors(rules);
        }

        public ExecutableDocument Document { get; }

        public IReadOnlyDictionary<string, object?>? VariableValues { get; }

        public TypeTracker Tracker { get; protected set; }

        public ExtensionData Extensions { get; } = new ExtensionData();

        public ISchema Schema { get; }

        public void Error(string code, string message, params INode[] nodes)
        {
            _errors.Add(new ValidationError(code, message, nodes));
        }

        public void Error(string code, string message, INode node)
        {
            _errors.Add(new ValidationError(code, message, node));
        }

        public void Error(string code, string message, IEnumerable<INode> nodes)
        {
            _errors.Add(new ValidationError(code, message, nodes));
        }

        public List<VariableUsage> GetVariables(
            INode rootNode)
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
            OperationDefinition operation)
        {
            if (_variables.TryGetValue(operation, out var results)) return results;

            var usages = GetVariables(operation);

            foreach (var fragment in GetRecursivelyReferencedFragments(operation))
                usages.AddRange(GetVariables(fragment));

            _variables[operation] = usages;

            return usages;
        }

        public FragmentDefinition? GetFragment(string name)
        {
            return Document.FragmentDefinitions
                ?.SingleOrDefault(f => f.FragmentName == name);
        }

        public List<FragmentSpread> GetFragmentSpreads(SelectionSet node)
        {
            var spreads = new List<FragmentSpread>();

            var setsToVisit = new Stack<SelectionSet>(new[] {node});

            while (setsToVisit.Count > 0)
            {
                var set = setsToVisit.Pop();

                foreach (var selection in set.Selections)
                    switch (selection)
                    {
                        case FragmentSpread spread:
                            spreads.Add(spread);
                            break;
                        case InlineFragment inlineFragment:
                        {
                            if (inlineFragment.SelectionSet != null)
                                setsToVisit.Push(inlineFragment.SelectionSet);
                            break;
                        }
                        /*
                        case OperationDefinition operationDefinition:
                        {
                            if (operationDefinition.SelectionSet != null)
                                setsToVisit.Push(operationDefinition.SelectionSet);
                            break;
                        }
                        */
                        case FieldSelection fieldSelection:
                        {
                            if (fieldSelection.SelectionSet != null) setsToVisit.Push(fieldSelection.SelectionSet);
                            break;
                        }
                    }
            }

            return spreads;
        }

        public IEnumerable<FragmentDefinition> GetRecursivelyReferencedFragments(
            OperationDefinition operation)
        {
            if (_fragments.TryGetValue(operation, out var results))
                return results;

            var fragments = new List<FragmentDefinition>();
            var nodesToVisit = new Stack<SelectionSet>(new[]
            {
                operation.SelectionSet
            });
            var collectedNames = new Dictionary<Name, bool>();

            while (nodesToVisit.Count > 0)
            {
                var node = nodesToVisit.Pop();

                foreach (var spread in GetFragmentSpreads(node))
                {
                    var fragName = spread.FragmentName;
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

        public override Argument BeginVisitArgument(Argument argument)
        {
            {
                Tracker.EnterArgument?.Invoke(argument);
            }

            return base.BeginVisitArgument(argument);
        }

        public override BooleanValue BeginVisitBooleanValue(
            BooleanValue value)
        {
            {
                Tracker.EnterBooleanValue?.Invoke(value);
            }

            return base.BeginVisitBooleanValue(value);
        }

        public override Directive BeginVisitDirective(Directive directive)
        {
            Tracker.EnterDirective?.Invoke(directive);

            var _ = base.BeginVisitDirective(directive);

            Tracker.LeaveDirective?.Invoke(_);
            return _;
        }

        public override EnumValue BeginVisitEnumValue(EnumValue value)
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

        public override FieldSelection BeginVisitFieldSelection(
            FieldSelection selection)
        {
            Tracker.EnterFieldSelection?.Invoke(selection);

            return base.BeginVisitFieldSelection(selection);
        }

        public override FloatValue BeginVisitFloatValue(
            FloatValue value)
        {
            {
                Tracker.EnterFloatValue?.Invoke(value);
            }

            return base.BeginVisitFloatValue(value);
        }

        public override FragmentDefinition BeginVisitFragmentDefinition(
            FragmentDefinition node)
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

        public override FragmentSpread BeginVisitFragmentSpread(
            FragmentSpread fragmentSpread)
        {
            {
                Tracker.EnterFragmentSpread?.Invoke(fragmentSpread);
            }

            return base.BeginVisitFragmentSpread(fragmentSpread);
        }

        public override InlineFragment BeginVisitInlineFragment(
            InlineFragment inlineFragment)
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

        public override IntValue BeginVisitIntValue(IntValue value)
        {
            {
                Tracker.EnterIntValue?.Invoke(value);
            }

            return base.BeginVisitIntValue(value);
        }

        public override ListValue BeginVisitListValue(ListValue node)
        {
            {
                Tracker.EnterListValue?.Invoke(node);
            }

            return base.BeginVisitListValue(node);
        }
        
        public override NamedType BeginVisitNamedType(
            NamedType typeCondition)
        {
            {
                Tracker.EnterNamedType?.Invoke(typeCondition);
            }

            return base.BeginVisitNamedType(typeCondition);
        }

        public override INode BeginVisitNode(INode node)
        {
            Tracker.EnterNode?.Invoke(node);
            return base.BeginVisitNode(node);
        }

        public override ObjectValue BeginVisitObjectValue(
            ObjectValue node)
        {
            {
                Tracker.EnterObjectValue?.Invoke(node);
            }

            return base.BeginVisitObjectValue(node);
        }

        public override ObjectField BeginVisitObjectField(ObjectField node)
        {
            Tracker.EnterObjectField?.Invoke(node);
            return base.BeginVisitObjectField(node);
        }

        public override OperationDefinition BeginVisitOperationDefinition(
            OperationDefinition definition)
        {
            {
                Tracker.EnterOperationDefinition?.Invoke(definition);
            }

            return base.BeginVisitOperationDefinition(definition);
        }

        public override SelectionSet BeginVisitSelectionSet(
            SelectionSet selectionSet)
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

        public override StringValue BeginVisitStringValue(
            StringValue value)
        {
            {
                Tracker.EnterStringValue?.Invoke(value);
            }

            return base.BeginVisitStringValue(value);
        }

        public override Variable BeginVisitVariable(Variable variable)
        {
            Tracker.EnterVariable?.Invoke(variable);
            return base.BeginVisitVariable(variable);
        }

        public override VariableDefinition BeginVisitVariableDefinition(
            VariableDefinition node)
        {
            Tracker.EnterVariableDefinition?.Invoke(node);

            var _ = base.BeginVisitVariableDefinition(node);

            Tracker.LeaveVariableDefinition?.Invoke(node);
            return _;
        }

        public override Argument EndVisitArgument(Argument argument)
        {
            {
                Tracker.LeaveArgument?.Invoke(argument);
            }

            return base.EndVisitArgument(argument);
        }

        public override FieldSelection EndVisitFieldSelection(
            FieldSelection selection)
        {
            {
                Tracker.LeaveFieldSelection?.Invoke(selection);
            }

            return base.EndVisitFieldSelection(selection);
        }

        public override ListValue EndVisitListValue(ListValue node)
        {
            {
                Tracker.LeaveListValue?.Invoke(node);
            }

            return base.EndVisitListValue(node);
        }

        public override ObjectValue EndVisitObjectValue(ObjectValue node)
        {
            {
                Tracker.LeaveObjectValue?.Invoke(node);
            }

            return base.EndVisitObjectValue(node);
        }

        public override OperationDefinition EndVisitOperationDefinition(
            OperationDefinition definition)
        {
            {
                Tracker.LeaveOperationDefinition?.Invoke(definition);
            }

            return base.EndVisitOperationDefinition(definition);
        }

        public override Variable EndVisitVariable(Variable variable)
        {
            Tracker.LeaveVariable?.Invoke(variable);
            return base.EndVisitVariable(variable);
        }

        public ValidationResult Validate()
        {
            Visit(Document);
            return BuildResult();
        }

        public override void Visit(ExecutableDocument document)
        {
            Tracker.EnterDocument?.Invoke(document);

            base.Visit(document);

            Tracker.LeaveDocument?.Invoke(document);
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