using System.Collections.Generic;
using System.Linq;
using GraphQLParser;
using GraphQLParser.AST;
using tanka.graphql.type;

namespace tanka.graphql.validation
{
    public class DocumentRulesVisitor : GraphQLAstVisitor
    {
        private readonly Dictionary<ASTNodeKind, List<IRule>> _visitorMap;
        private List<(IRule, IEnumerable<ValidationError>)> _errors = new List<(IRule, IEnumerable<ValidationError>)>();

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
            foreach (var rule in rules) CollectErrors(rule, rule.Visit(ast));

            base.Visit(ast);
        }

        private void CollectErrors(IRule rule, IEnumerable<ValidationError> validationErrors)
        {
            _errors.Add((rule, validationErrors));
        }

        public override GraphQLName BeginVisitAlias(GraphQLName alias)
        {
            var rules = GetRules(alias);
            foreach (var rule in rules) CollectErrors(rule, rule.BeginVisitAlias(alias));

            return base.BeginVisitAlias(alias);
        }

        public override GraphQLArgument BeginVisitArgument(GraphQLArgument argument)
        {
            var rules = GetRules(argument);
            foreach (var rule in rules) CollectErrors(rule, rule.BeginVisitArgument(argument));

            return base.BeginVisitArgument(argument);
        }

        public override IEnumerable<GraphQLArgument> BeginVisitArguments(IEnumerable<GraphQLArgument> arguments)
        {
            var rules = GetRules(ASTNodeKind.Argument);
            foreach (var rule in rules) CollectErrors(rule, rule.BeginVisitArguments(arguments));

            return base.BeginVisitArguments(arguments);
        }

        public override GraphQLScalarValue BeginVisitBooleanValue(
            GraphQLScalarValue value)
        {
            var rules = GetRules(value);
            foreach (var rule in rules) CollectErrors(rule, rule.BeginVisitBooleanValue(value));

            return base.BeginVisitBooleanValue(value);
        }

        public override GraphQLDirective BeginVisitDirective(GraphQLDirective directive)
        {
            var rules = GetRules(directive);
            foreach (var rule in rules) 
                CollectErrors(rule, rule.BeginVisitDirective(directive));

            return base.BeginVisitDirective(directive);
        }

        public override GraphQLScalarValue BeginVisitEnumValue(GraphQLScalarValue value)
        {
            var rules = GetRules(value);
            foreach (var rule in rules) 
                CollectErrors(rule, rule.BeginVisitEnumValue(value));

            return base.BeginVisitEnumValue(value);
        }

        public override GraphQLFieldSelection BeginVisitFieldSelection(
            GraphQLFieldSelection selection)
        {
            var rules = GetRules(selection);
            foreach (var rule in rules) 
                CollectErrors(rule, rule.BeginVisitFieldSelection(selection));

            return base.BeginVisitFieldSelection(selection);
        }

        public override GraphQLScalarValue BeginVisitFloatValue(
            GraphQLScalarValue value)
        {
            var rules = GetRules(value);
            foreach (var rule in rules) 
                CollectErrors(rule, rule.BeginVisitFloatValue(value));

            return base.BeginVisitFloatValue(value);
        }

        public override GraphQLFragmentDefinition BeginVisitFragmentDefinition(
            GraphQLFragmentDefinition node)
        {
            var rules = GetRules(node);
            foreach (var rule in rules) 
                CollectErrors(rule, rule.BeginVisitFragmentDefinition(node));

            return base.BeginVisitFragmentDefinition(node);
        }

        public override GraphQLFragmentSpread BeginVisitFragmentSpread(
            GraphQLFragmentSpread fragmentSpread)
        {
            var rules = GetRules(fragmentSpread);
            foreach (var rule in rules) 
                CollectErrors(rule, rule.BeginVisitFragmentSpread(fragmentSpread));

            return base.BeginVisitFragmentSpread(fragmentSpread);
        }

        public override GraphQLInlineFragment BeginVisitInlineFragment(
            GraphQLInlineFragment inlineFragment)
        {
            var rules = GetRules(inlineFragment);
            foreach (var rule in rules) 
                CollectErrors(rule, rule.BeginVisitInlineFragment(inlineFragment));

            return base.BeginVisitInlineFragment(inlineFragment);
        }

        public override GraphQLScalarValue BeginVisitIntValue(GraphQLScalarValue value)
        {
            var rules = GetRules(value);
            foreach (var rule in rules) 
                CollectErrors(rule, rule.BeginVisitIntValue(value));

            return base.BeginVisitIntValue(value);
        }

        public override GraphQLName BeginVisitName(GraphQLName name)
        {
            var rules = GetRules(name);
            foreach (var rule in rules) 
                CollectErrors(rule, rule.BeginVisitName(name));

            return base.BeginVisitName(name);
        }

        public override GraphQLNamedType BeginVisitNamedType(
            GraphQLNamedType typeCondition)
        {
            var rules = GetRules(typeCondition);
            foreach (var rule in rules) 
                CollectErrors(rule, rule.BeginVisitNamedType(typeCondition));

            return base.BeginVisitNamedType(typeCondition);
        }

        public override GraphQLOperationDefinition BeginVisitOperationDefinition(
            GraphQLOperationDefinition definition)
        {
            var rules = GetRules(definition);
            foreach (var rule in rules) 
                CollectErrors(rule, rule.BeginVisitOperationDefinition(definition));

            return base.BeginVisitOperationDefinition(definition);
        }

        public override GraphQLOperationDefinition EndVisitOperationDefinition(
            GraphQLOperationDefinition definition)
        {
            var rules = GetRules(definition);
            foreach (var rule in rules) 
                CollectErrors(rule, rule.BeginVisitOperationDefinition(definition));

            return base.EndVisitOperationDefinition(definition);
        }

        public override GraphQLSelectionSet BeginVisitSelectionSet(
            GraphQLSelectionSet selectionSet)
        {
            var rules = GetRules(selectionSet);
            foreach (var rule in rules) 
                CollectErrors(rule, rule.BeginVisitSelectionSet(selectionSet));

            return base.BeginVisitSelectionSet(selectionSet);
        }

        public override GraphQLScalarValue BeginVisitStringValue(
            GraphQLScalarValue value)
        {
            var rules = GetRules(value);
            foreach (var rule in rules) 
                CollectErrors(rule, rule.BeginVisitStringValue(value));

            return base.BeginVisitStringValue(value);
        }

        public override GraphQLVariable BeginVisitVariable(GraphQLVariable variable)
        {
            var rules = GetRules(variable);
            foreach (var rule in rules) 
                CollectErrors(rule, rule.BeginVisitVariable(variable));

            return base.BeginVisitVariable(variable);
        }

        public override GraphQLVariableDefinition BeginVisitVariableDefinition(
            GraphQLVariableDefinition node)
        {
            var rules = GetRules(node);
            foreach (var rule in rules) 
                CollectErrors(rule, rule.BeginVisitVariableDefinition(node));

            return base.BeginVisitVariableDefinition(node);
        }

        public override IEnumerable<GraphQLVariableDefinition> BeginVisitVariableDefinitions(
            IEnumerable<GraphQLVariableDefinition> variableDefinitions)
        {
            var rules = GetRules(ASTNodeKind.VariableDefinition);

            foreach (var rule in rules) 
                CollectErrors(rule, rule.BeginVisitVariableDefinitions(variableDefinitions));

            return base.BeginVisitVariableDefinitions(variableDefinitions);
        }

        public override GraphQLArgument EndVisitArgument(GraphQLArgument argument)
        {
            var rules = GetRules(argument);
            foreach (var rule in rules) 
                CollectErrors(rule, rule.EndVisitArgument(argument));

            return base.EndVisitArgument(argument);
        }

        public override GraphQLFieldSelection EndVisitFieldSelection(
            GraphQLFieldSelection selection)
        {
            var rules = GetRules(selection);
            foreach (var rule in rules) 
                CollectErrors(rule, rule.EndVisitFieldSelection(selection));

            return base.EndVisitFieldSelection(selection);
        }

        public override GraphQLVariable EndVisitVariable(GraphQLVariable variable)
        {
            var rules = GetRules(variable);
            foreach (var rule in rules) 
                CollectErrors(rule, rule.EndVisitVariable(variable));

            return base.EndVisitVariable(variable);
        }

        public override GraphQLObjectField BeginVisitObjectField(
            GraphQLObjectField node)
        {
            var rules = GetRules(node);
            foreach (var rule in rules) 
                CollectErrors(rule, rule.BeginVisitObjectField(node));

            return base.BeginVisitObjectField(node);
        }

        public override GraphQLObjectValue BeginVisitObjectValue(
            GraphQLObjectValue node)
        {
            var rules = GetRules(node);
            foreach (var rule in rules) 
                CollectErrors(rule, rule.BeginVisitObjectValue(node));

            return base.BeginVisitObjectValue(node);
        }

        public override GraphQLObjectValue EndVisitObjectValue(GraphQLObjectValue node)
        {
            var rules = GetRules(node);
            foreach (var rule in rules) 
                CollectErrors(rule, rule.EndVisitObjectValue(node));

            return base.EndVisitObjectValue(node);
        }

        public override GraphQLListValue EndVisitListValue(GraphQLListValue node)
        {
            var rules = GetRules(node);
            foreach (var rule in rules) 
                CollectErrors(rule, rule.EndVisitListValue(node));

            return base.EndVisitListValue(node);
        }

        private ValidationResult BuildResult()
        {
            return new ValidationResult()
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