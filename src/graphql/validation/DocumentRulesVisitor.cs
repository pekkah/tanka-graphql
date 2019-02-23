using System.Collections.Generic;
using System.Linq;
using GraphQLParser.AST;
using tanka.graphql.language;
using tanka.graphql.type;

namespace tanka.graphql.validation
{
    public class DocumentRulesVisitor : Visitor, IValidationContext
    {
        private readonly List<ValidationError> _errors =
            new List<ValidationError>();

        private readonly Dictionary<ASTNodeKind, List<IRule>> _visitorMap;


        public DocumentRulesVisitor(
            Dictionary<ASTNodeKind, List<IRule>> ruleMap,
            ISchema schema,
            GraphQLDocument document,
            Dictionary<string, object> variableValues = null)
        {
            Schema = schema;
            Document = document;
            VariableValues = variableValues;
            _visitorMap = ruleMap;
        }

        public GraphQLDocument Document { get; }

        public Dictionary<string, object> VariableValues { get; }

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

        public static Dictionary<ASTNodeKind, List<IRule>> InitializeRuleActionMap(IEnumerable<IRule> rules)
        {
            var visitors = new Dictionary<ASTNodeKind, List<IRule>>();

            foreach (var rule in rules)
            foreach (var nodeKind in rule.AppliesToNodeKinds)
            {
                if (!visitors.ContainsKey(nodeKind))
                    visitors[nodeKind] = new List<IRule>();

                visitors[nodeKind].Add(rule);
            }

            return visitors;
        }

        public ValidationResult Validate()
        {
            Visit(Document);
            return BuildResult();
        }

        public override void Visit(GraphQLDocument document)
        {
            var rules = GetRules(document);
            foreach (var rule in rules)
                rule.Visit(document, this);

            base.Visit(document);
        }

        public override GraphQLName BeginVisitAlias(GraphQLName alias)
        {
            var rules = GetRules(alias);
            foreach (var rule in rules)
                rule.BeginVisitAlias(alias, this);

            return base.BeginVisitAlias(alias);
        }

        public override GraphQLArgument BeginVisitArgument(GraphQLArgument argument)
        {
            var rules = GetRules(argument);
            foreach (var rule in rules)
                rule.BeginVisitArgument(argument, this);

            return base.BeginVisitArgument(argument);
        }

        public override IEnumerable<GraphQLArgument> BeginVisitArguments(IEnumerable<GraphQLArgument> arguments)
        {
            var rules = GetRules(ASTNodeKind.Argument);
            foreach (var rule in rules)
                rule.BeginVisitArguments(arguments, this);

            return base.BeginVisitArguments(arguments);
        }

        public override GraphQLScalarValue BeginVisitBooleanValue(
            GraphQLScalarValue value)
        {
            var rules = GetRules(value);
            foreach (var rule in rules)
                rule.BeginVisitBooleanValue(value, this);

            return base.BeginVisitBooleanValue(value);
        }

        public override GraphQLDirective BeginVisitDirective(GraphQLDirective directive)
        {
            var rules = GetRules(directive).ToList();
            foreach (var rule in rules)
                rule.BeginVisitDirective(directive, this);

            var _ = base.BeginVisitDirective(directive);

            foreach (var rule in rules)
                rule.EndVisitDirective(directive, this);

            return _;
        }

        public override GraphQLScalarValue BeginVisitEnumValue(GraphQLScalarValue value)
        {
            var rules = GetRules(value).ToList();
            foreach (var rule in rules)
                rule.BeginVisitEnumValue(value, this);

            var _ = base.BeginVisitEnumValue(value);

            foreach (var rule in rules)
                rule.EndVisitEnumValue(value, this);

            return _;
        }

        public override GraphQLFieldSelection BeginVisitFieldSelection(
            GraphQLFieldSelection selection)
        {
            var rules = GetRules(selection);
            foreach (var rule in rules)
                rule.BeginVisitFieldSelection(selection, this);

            return base.BeginVisitFieldSelection(selection);
        }

        public override GraphQLScalarValue BeginVisitFloatValue(
            GraphQLScalarValue value)
        {
            var rules = GetRules(value);
            foreach (var rule in rules)
                rule.BeginVisitFloatValue(value, this);

            return base.BeginVisitFloatValue(value);
        }

        public override GraphQLFragmentDefinition BeginVisitFragmentDefinition(
            GraphQLFragmentDefinition node)
        {
            var rules = GetRules(node).ToList();
            foreach (var rule in rules)
                rule.BeginVisitFragmentDefinition(node, this);

            var result = base.BeginVisitFragmentDefinition(node);

            foreach (var rule in rules)
                rule.EndVisitFragmentDefinition(node, this);

            return result;
        }

        public override GraphQLFragmentSpread BeginVisitFragmentSpread(
            GraphQLFragmentSpread fragmentSpread)
        {
            var rules = GetRules(fragmentSpread);
            foreach (var rule in rules)
                rule.BeginVisitFragmentSpread(fragmentSpread, this);

            return base.BeginVisitFragmentSpread(fragmentSpread);
        }

        public override GraphQLInlineFragment BeginVisitInlineFragment(
            GraphQLInlineFragment inlineFragment)
        {
            var rules = GetRules(inlineFragment).ToList();
            foreach (var rule in rules)
                rule.BeginVisitInlineFragment(inlineFragment, this);

            var _ = base.BeginVisitInlineFragment(inlineFragment);

            foreach (var rule in rules)
                rule.EndVisitInlineFragment(inlineFragment, this);

            return _;
        }

        public override GraphQLScalarValue BeginVisitIntValue(GraphQLScalarValue value)
        {
            var rules = GetRules(value);
            foreach (var rule in rules)
                rule.BeginVisitIntValue(value, this);

            return base.BeginVisitIntValue(value);
        }

        public override GraphQLName BeginVisitName(GraphQLName name)
        {
            var rules = GetRules(name);
            foreach (var rule in rules)
                rule.BeginVisitName(name, this);

            return base.BeginVisitName(name);
        }

        public override GraphQLNamedType BeginVisitNamedType(
            GraphQLNamedType typeCondition)
        {
            var rules = GetRules(typeCondition);
            foreach (var rule in rules)
                rule.BeginVisitNamedType(typeCondition, this);

            return base.BeginVisitNamedType(typeCondition);
        }

        public override GraphQLOperationDefinition BeginVisitOperationDefinition(
            GraphQLOperationDefinition definition)
        {
            var rules = GetRules(definition);
            foreach (var rule in rules)
                rule.BeginVisitOperationDefinition(definition, this);

            return base.BeginVisitOperationDefinition(definition);
        }

        public override GraphQLOperationDefinition EndVisitOperationDefinition(
            GraphQLOperationDefinition definition)
        {
            var rules = GetRules(definition);
            foreach (var rule in rules)
                rule.EndVisitOperationDefinition(definition, this);

            return base.EndVisitOperationDefinition(definition);
        }

        public override GraphQLSelectionSet BeginVisitSelectionSet(
            GraphQLSelectionSet selectionSet)
        {
            var rules = GetRules(selectionSet).ToList();
            foreach (var rule in rules)
                rule.BeginVisitSelectionSet(selectionSet, this);

            var _ = base.BeginVisitSelectionSet(selectionSet);

            foreach (var rule in rules)
                rule.EndVisitSelectionSet(selectionSet, this);

            return _;
        }

        public override GraphQLScalarValue BeginVisitStringValue(
            GraphQLScalarValue value)
        {
            var rules = GetRules(value);
            foreach (var rule in rules)
                rule.BeginVisitStringValue(value, this);

            return base.BeginVisitStringValue(value);
        }

        public override GraphQLVariable BeginVisitVariable(GraphQLVariable variable)
        {
            var rules = GetRules(variable);
            foreach (var rule in rules)
                rule.BeginVisitVariable(variable, this);

            return base.BeginVisitVariable(variable);
        }

        public override GraphQLVariableDefinition BeginVisitVariableDefinition(
            GraphQLVariableDefinition node)
        {
            var rules = GetRules(node).ToList();
            foreach (var rule in rules)
                rule.BeginVisitVariableDefinition(node, this);

            var _ = base.BeginVisitVariableDefinition(node);

            foreach (var rule in rules)
                rule.EndVisitVariableDefinition(node, this);

            return _;
        }

        public override IEnumerable<GraphQLVariableDefinition> BeginVisitVariableDefinitions(
            IEnumerable<GraphQLVariableDefinition> variableDefinitions)
        {
            var rules = GetRules(ASTNodeKind.VariableDefinition);

            foreach (var rule in rules)
                rule.BeginVisitVariableDefinitions(variableDefinitions, this);

            return base.BeginVisitVariableDefinitions(variableDefinitions);
        }

        public override GraphQLArgument EndVisitArgument(GraphQLArgument argument)
        {
            var rules = GetRules(argument);
            foreach (var rule in rules)
                rule.EndVisitArgument(argument, this);

            return base.EndVisitArgument(argument);
        }

        public override GraphQLFieldSelection EndVisitFieldSelection(
            GraphQLFieldSelection selection)
        {
            var rules = GetRules(selection);
            foreach (var rule in rules)
                rule.EndVisitFieldSelection(selection, this);

            return base.EndVisitFieldSelection(selection);
        }

        public override GraphQLVariable EndVisitVariable(GraphQLVariable variable)
        {
            var rules = GetRules(variable);
            foreach (var rule in rules)
                rule.EndVisitVariable(variable, this);

            return base.EndVisitVariable(variable);
        }

        public override GraphQLObjectField BeginVisitObjectField(
            GraphQLObjectField node)
        {
            var rules = GetRules(node).ToList();
            foreach (var rule in rules)
                rule.BeginVisitObjectField(node, this);

            var _ = base.BeginVisitObjectField(node);

            foreach (var rule in rules)
                rule.EndVisitObjectField(node, this);

            return _;
        }

        public override GraphQLObjectValue BeginVisitObjectValue(
            GraphQLObjectValue node)
        {
            var rules = GetRules(node);
            foreach (var rule in rules)
                rule.BeginVisitObjectValue(node, this);

            return base.BeginVisitObjectValue(node);
        }

        public override GraphQLObjectValue EndVisitObjectValue(GraphQLObjectValue node)
        {
            var rules = GetRules(node);
            foreach (var rule in rules)
                rule.EndVisitObjectValue(node, this);

            return base.EndVisitObjectValue(node);
        }

        public override ASTNode BeginVisitNode(ASTNode node)
        {
            return base.BeginVisitNode(node);
        }

        public override GraphQLListValue BeginVisitListValue(GraphQLListValue node)
        {
            var rules = GetRules(node);
            foreach (var rule in rules)
                rule.BeginVisitListValue(node, this);

            return base.BeginVisitListValue(node);
        }

        public override GraphQLListValue EndVisitListValue(GraphQLListValue node)
        {
            var rules = GetRules(node);
            foreach (var rule in rules)
                rule.EndVisitListValue(node, this);

            return base.EndVisitListValue(node);
        }

        private ValidationResult BuildResult()
        {
            return new ValidationResult
            {
                Errors = _errors
            };
        }

        private IEnumerable<IRule> GetRules(ASTNode node)
        {
            var nodeKind = node.Kind;
            return GetRules(nodeKind);
        }

        private IEnumerable<IRule> GetRules(ASTNodeKind nodeKind)
        {
            if (!_visitorMap.ContainsKey(nodeKind))
                return Enumerable.Empty<IRule>();

            return _visitorMap[nodeKind];
        }
    }
}