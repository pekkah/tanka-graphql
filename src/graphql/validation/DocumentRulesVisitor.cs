using System.Collections.Generic;
using System.Linq;
using GraphQLParser;
using GraphQLParser.AST;
using tanka.graphql.type;

namespace tanka.graphql.validation
{
    public class DocumentRulesVisitor : GraphQLAstVisitor, IValidationContext
    {
        private readonly Dictionary<ASTNodeKind, List<IRule>> _visitorMap;

        private readonly List<(IRule, IEnumerable<ValidationError>)> _errors =
            new List<(IRule, IEnumerable<ValidationError>)>();

        public DocumentRulesVisitor(
            IEnumerable<IRule> rules,
            ISchema schema,
            GraphQLDocument document,
            Dictionary<string, object> variableValues = null)
            : this(InitializeRuleActionMap(rules), schema, document, variableValues)
        {
        }


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

        public override void Visit(GraphQLDocument ast)
        {
            var rules = GetRules(ast);
            foreach (var rule in rules)
                CollectErrors(rule, rule.Visit(ast, this));

            base.Visit(ast);
        }

        public override GraphQLName BeginVisitAlias(GraphQLName alias)
        {
            var rules = GetRules(alias);
            foreach (var rule in rules)
                CollectErrors(rule, rule.BeginVisitAlias(alias, this));

            return base.BeginVisitAlias(alias);
        }

        public override GraphQLArgument BeginVisitArgument(GraphQLArgument argument)
        {
            var rules = GetRules(argument);
            foreach (var rule in rules)
                CollectErrors(rule, rule.BeginVisitArgument(argument, this));

            return base.BeginVisitArgument(argument);
        }

        public override IEnumerable<GraphQLArgument> BeginVisitArguments(IEnumerable<GraphQLArgument> arguments)
        {
            var rules = GetRules(ASTNodeKind.Argument);
            foreach (var rule in rules)
                CollectErrors(rule, rule.BeginVisitArguments(arguments, this));

            return base.BeginVisitArguments(arguments);
        }

        public override GraphQLScalarValue BeginVisitBooleanValue(
            GraphQLScalarValue value)
        {
            var rules = GetRules(value);
            foreach (var rule in rules)
                CollectErrors(rule, rule.BeginVisitBooleanValue(value, this));

            return base.BeginVisitBooleanValue(value);
        }

        public override GraphQLDirective BeginVisitDirective(GraphQLDirective directive)
        {
            var rules = GetRules(directive);
            foreach (var rule in rules)
                CollectErrors(rule, rule.BeginVisitDirective(directive, this));

            return base.BeginVisitDirective(directive);
        }

        public override GraphQLScalarValue BeginVisitEnumValue(GraphQLScalarValue value)
        {
            var rules = GetRules(value);
            foreach (var rule in rules)
                CollectErrors(rule, rule.BeginVisitEnumValue(value, this));

            return base.BeginVisitEnumValue(value);
        }

        public override GraphQLFieldSelection BeginVisitFieldSelection(
            GraphQLFieldSelection selection)
        {
            var rules = GetRules(selection);
            foreach (var rule in rules)
                CollectErrors(rule, rule.BeginVisitFieldSelection(selection, this));

            return base.BeginVisitFieldSelection(selection);
        }

        public override GraphQLScalarValue BeginVisitFloatValue(
            GraphQLScalarValue value)
        {
            var rules = GetRules(value);
            foreach (var rule in rules)
                CollectErrors(rule, rule.BeginVisitFloatValue(value, this));

            return base.BeginVisitFloatValue(value);
        }

        public override GraphQLFragmentDefinition BeginVisitFragmentDefinition(
            GraphQLFragmentDefinition node)
        {
            var rules = GetRules(node);
            foreach (var rule in rules)
                CollectErrors(rule, rule.BeginVisitFragmentDefinition(node, this));

            return base.BeginVisitFragmentDefinition(node);
        }

        public override GraphQLFragmentSpread BeginVisitFragmentSpread(
            GraphQLFragmentSpread fragmentSpread)
        {
            var rules = GetRules(fragmentSpread);
            foreach (var rule in rules)
                CollectErrors(rule, rule.BeginVisitFragmentSpread(fragmentSpread, this));

            return base.BeginVisitFragmentSpread(fragmentSpread);
        }

        public override GraphQLInlineFragment BeginVisitInlineFragment(
            GraphQLInlineFragment inlineFragment)
        {
            var rules = GetRules(inlineFragment);
            foreach (var rule in rules)
                CollectErrors(rule, rule.BeginVisitInlineFragment(inlineFragment, this));

            return base.BeginVisitInlineFragment(inlineFragment);
        }

        public override GraphQLScalarValue BeginVisitIntValue(GraphQLScalarValue value)
        {
            var rules = GetRules(value);
            foreach (var rule in rules)
                CollectErrors(rule, rule.BeginVisitIntValue(value, this));

            return base.BeginVisitIntValue(value);
        }

        public override GraphQLName BeginVisitName(GraphQLName name)
        {
            var rules = GetRules(name);
            foreach (var rule in rules)
                CollectErrors(rule, rule.BeginVisitName(name, this));

            return base.BeginVisitName(name);
        }

        public override GraphQLNamedType BeginVisitNamedType(
            GraphQLNamedType typeCondition)
        {
            var rules = GetRules(typeCondition);
            foreach (var rule in rules)
                CollectErrors(rule, rule.BeginVisitNamedType(typeCondition, this));

            return base.BeginVisitNamedType(typeCondition);
        }

        public override GraphQLOperationDefinition BeginVisitOperationDefinition(
            GraphQLOperationDefinition definition)
        {
            var rules = GetRules(definition);
            foreach (var rule in rules)
                CollectErrors(rule, rule.BeginVisitOperationDefinition(definition, this));

            return base.BeginVisitOperationDefinition(definition);
        }

        public override GraphQLOperationDefinition EndVisitOperationDefinition(
            GraphQLOperationDefinition definition)
        {
            var rules = GetRules(definition);
            foreach (var rule in rules)
                CollectErrors(rule, rule.BeginVisitOperationDefinition(definition, this));

            return base.EndVisitOperationDefinition(definition);
        }

        public override GraphQLSelectionSet BeginVisitSelectionSet(
            GraphQLSelectionSet selectionSet)
        {
            var rules = GetRules(selectionSet);
            foreach (var rule in rules)
                CollectErrors(rule, rule.BeginVisitSelectionSet(selectionSet, this));

            return base.BeginVisitSelectionSet(selectionSet);
        }

        public override GraphQLScalarValue BeginVisitStringValue(
            GraphQLScalarValue value)
        {
            var rules = GetRules(value);
            foreach (var rule in rules)
                CollectErrors(rule, rule.BeginVisitStringValue(value, this));

            return base.BeginVisitStringValue(value);
        }

        public override GraphQLVariable BeginVisitVariable(GraphQLVariable variable)
        {
            var rules = GetRules(variable);
            foreach (var rule in rules)
                CollectErrors(rule, rule.BeginVisitVariable(variable, this));

            return base.BeginVisitVariable(variable);
        }

        public override GraphQLVariableDefinition BeginVisitVariableDefinition(
            GraphQLVariableDefinition node)
        {
            var rules = GetRules(node);
            foreach (var rule in rules)
                CollectErrors(rule, rule.BeginVisitVariableDefinition(node, this));

            return base.BeginVisitVariableDefinition(node);
        }

        public override IEnumerable<GraphQLVariableDefinition> BeginVisitVariableDefinitions(
            IEnumerable<GraphQLVariableDefinition> variableDefinitions)
        {
            var rules = GetRules(ASTNodeKind.VariableDefinition);

            foreach (var rule in rules)
                CollectErrors(rule, rule.BeginVisitVariableDefinitions(variableDefinitions, this));

            return base.BeginVisitVariableDefinitions(variableDefinitions);
        }

        public override GraphQLArgument EndVisitArgument(GraphQLArgument argument)
        {
            var rules = GetRules(argument);
            foreach (var rule in rules)
                CollectErrors(rule, rule.EndVisitArgument(argument, this));

            return base.EndVisitArgument(argument);
        }

        public override GraphQLFieldSelection EndVisitFieldSelection(
            GraphQLFieldSelection selection)
        {
            var rules = GetRules(selection);
            foreach (var rule in rules)
                CollectErrors(rule, rule.EndVisitFieldSelection(selection, this));

            return base.EndVisitFieldSelection(selection);
        }

        public override GraphQLVariable EndVisitVariable(GraphQLVariable variable)
        {
            var rules = GetRules(variable);
            foreach (var rule in rules)
                CollectErrors(rule, rule.EndVisitVariable(variable, this));

            return base.EndVisitVariable(variable);
        }

        public override GraphQLObjectField BeginVisitObjectField(
            GraphQLObjectField node)
        {
            var rules = GetRules(node);
            foreach (var rule in rules)
                CollectErrors(rule, rule.BeginVisitObjectField(node, this));

            return base.BeginVisitObjectField(node);
        }

        public override GraphQLObjectValue BeginVisitObjectValue(
            GraphQLObjectValue node)
        {
            var rules = GetRules(node);
            foreach (var rule in rules)
                CollectErrors(rule, rule.BeginVisitObjectValue(node, this));

            return base.BeginVisitObjectValue(node);
        }

        public override GraphQLObjectValue EndVisitObjectValue(GraphQLObjectValue node)
        {
            var rules = GetRules(node);
            foreach (var rule in rules)
                CollectErrors(rule, rule.EndVisitObjectValue(node, this));

            return base.EndVisitObjectValue(node);
        }

        public override GraphQLListValue EndVisitListValue(GraphQLListValue node)
        {
            var rules = GetRules(node);
            foreach (var rule in rules)
                CollectErrors(rule, rule.EndVisitListValue(node, this));

            return base.EndVisitListValue(node);
        }

        private void CollectErrors(IRule rule, IEnumerable<ValidationError> validationErrors)
        {
            _errors.Add((rule, validationErrors));
        }

        private ValidationResult BuildResult()
        {
            return new ValidationResult
            {
                Errors = _errors.SelectMany(e => e.Item2).ToList()
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