using System;
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

        public RulesWalker(
            IEnumerable<CreateRule> rules,
            ISchema schema,
            GraphQLDocument document,
            Dictionary<string, object> variableValues = null)
        {
            Schema = schema;
            Document = document;
            VariableValues = variableValues;
            NodeVisitors = CreateVisitors(rules).ToList();

            //todo: this will break
            Tracker = NodeVisitors.First() as TypeTracker;
        }

        protected IEnumerable<RuleVisitor> NodeVisitors { get; set; }

        protected IEnumerable<RuleVisitor> CreateVisitors(IEnumerable<CreateRule> rules)
        {
            var createRules = new List<CreateRule>(rules);
            createRules.Insert(0, context => new TypeTracker(context.Schema));

            return createRules.Select(r => r(this));
        }

        public GraphQLDocument Document { get; }

        public IDictionary<string, object> VariableValues { get; }

        public TypeTracker Tracker { get; }

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

        public ValidationResult Validate()
        {
            Visit(Document);
            return BuildResult();
        }

        public override void Visit(GraphQLDocument document)
        {
            foreach (var visitor in NodeVisitors)
            {
                visitor.EnterDocument?.Invoke(document);
            }

            base.Visit(document);

            foreach (var visitor in NodeVisitors)
            {
                visitor.LeaveDocument?.Invoke(document);
            }
        }

        public override GraphQLName BeginVisitAlias(GraphQLName alias)
        {
            foreach (var visitor in NodeVisitors)
            {
                visitor.EnterAlias?.Invoke(alias);
            }

            return base.BeginVisitAlias(alias);
        }

        public override GraphQLArgument BeginVisitArgument(GraphQLArgument argument)
        {
            foreach (var visitor in NodeVisitors)
            {
                visitor.EnterArgument?.Invoke(argument);
            }

            return base.BeginVisitArgument(argument);
        }

        public override GraphQLScalarValue BeginVisitBooleanValue(
            GraphQLScalarValue value)
        {
            foreach (var visitor in NodeVisitors)
            {
                visitor.EnterBooleanValue?.Invoke(value);
            }

            return base.BeginVisitBooleanValue(value);
        }

        public override GraphQLDirective BeginVisitDirective(GraphQLDirective directive)
        {
            foreach (var visitor in NodeVisitors)
            {
                visitor.EnterDirective?.Invoke(directive);
            }

            var _ = base.BeginVisitDirective(directive);

            foreach (var visitor in NodeVisitors)
            {
                visitor.LeaveDirective?.Invoke(directive);
            }

            return _;
        }

        public override GraphQLScalarValue BeginVisitEnumValue(GraphQLScalarValue value)
        {
            foreach (var visitor in NodeVisitors)
            {
                visitor.EnterEnumValue?.Invoke(value);
            }

            var _ = base.BeginVisitEnumValue(value);

            foreach (var visitor in NodeVisitors)
            {
                visitor.LeaveEnumValue?.Invoke(value);
            }

            return _;
        }

        public override GraphQLFieldSelection BeginVisitFieldSelection(
            GraphQLFieldSelection selection)
        {
            foreach (var visitor in NodeVisitors)
            {
                visitor.EnterFieldSelection?.Invoke(selection);
            }

            return base.BeginVisitFieldSelection(selection);
        }

        public override GraphQLScalarValue BeginVisitFloatValue(
            GraphQLScalarValue value)
        {
            foreach (var visitor in NodeVisitors)
            {
                visitor.EnterFloatValue?.Invoke(value);
            }

            return base.BeginVisitFloatValue(value);
        }

        public override GraphQLFragmentDefinition BeginVisitFragmentDefinition(
            GraphQLFragmentDefinition node)
        {
            foreach (var visitor in NodeVisitors)
            {
                visitor.EnterFragmentDefinition?.Invoke(node);
            }

            var result = base.BeginVisitFragmentDefinition(node);

            foreach (var visitor in NodeVisitors)
            {
                visitor.LeaveFragmentDefinition?.Invoke(node);
            }

            return result;
        }

        public override GraphQLFragmentSpread BeginVisitFragmentSpread(
            GraphQLFragmentSpread fragmentSpread)
        {
            foreach (var visitor in NodeVisitors)
            {
                visitor.EnterFragmentSpread?.Invoke(fragmentSpread);
            }

            return base.BeginVisitFragmentSpread(fragmentSpread);
        }

        public override GraphQLInlineFragment BeginVisitInlineFragment(
            GraphQLInlineFragment inlineFragment)
        {
            foreach (var visitor in NodeVisitors)
            {
                visitor.EnterInlineFragment?.Invoke(inlineFragment);
            }

            var _ = base.BeginVisitInlineFragment(inlineFragment);

            foreach (var visitor in NodeVisitors)
            {
                visitor.LeaveInlineFragment?.Invoke(inlineFragment);
            }

            return _;
        }

        public override GraphQLScalarValue BeginVisitIntValue(GraphQLScalarValue value)
        {
            foreach (var visitor in NodeVisitors)
            {
                visitor.EnterIntValue?.Invoke(value);
            }

            return base.BeginVisitIntValue(value);
        }

        public override GraphQLName BeginVisitName(GraphQLName name)
        {
            foreach (var visitor in NodeVisitors)
            {
                visitor.EnterName?.Invoke(name);
            }

            return base.BeginVisitName(name);
        }

        public override GraphQLNamedType BeginVisitNamedType(
            GraphQLNamedType typeCondition)
        {
            foreach (var visitor in NodeVisitors)
            {
                visitor.EnterNamedType?.Invoke(typeCondition);
            }

            return base.BeginVisitNamedType(typeCondition);
        }

        public override GraphQLOperationDefinition BeginVisitOperationDefinition(
            GraphQLOperationDefinition definition)
        {
            foreach (var visitor in NodeVisitors)
            {
                visitor.EnterOperationDefinition?.Invoke(definition);
            }

            return base.BeginVisitOperationDefinition(definition);
        }

        public override GraphQLOperationDefinition EndVisitOperationDefinition(
            GraphQLOperationDefinition definition)
        {
            foreach (var visitor in NodeVisitors)
            {
                visitor.LeaveOperationDefinition?.Invoke(definition);
            }

            return base.EndVisitOperationDefinition(definition);
        }

        public override GraphQLSelectionSet BeginVisitSelectionSet(
            GraphQLSelectionSet selectionSet)
        {
            foreach (var visitor in NodeVisitors)
            {
                visitor.EnterSelectionSet?.Invoke(selectionSet);
            }

            var _ = base.BeginVisitSelectionSet(selectionSet);

            foreach (var visitor in NodeVisitors)
            {
                visitor.LeaveSelectionSet?.Invoke(selectionSet);
            }

            return _;
        }

        public override GraphQLScalarValue BeginVisitStringValue(
            GraphQLScalarValue value)
        {
            foreach (var visitor in NodeVisitors)
            {
                visitor.EnterStringValue?.Invoke(value);
            }

            return base.BeginVisitStringValue(value);
        }

        public override GraphQLVariable BeginVisitVariable(GraphQLVariable variable)
        {
            foreach (var visitor in NodeVisitors)
            {
                visitor.EnterVariable?.Invoke(variable);
            }

            return base.BeginVisitVariable(variable);
        }

        public override GraphQLVariableDefinition BeginVisitVariableDefinition(
            GraphQLVariableDefinition node)
        {
            foreach (var visitor in NodeVisitors)
            {
                visitor.EnterVariableDefinition?.Invoke(node);
            }

            var _ = base.BeginVisitVariableDefinition(node);

            foreach (var visitor in NodeVisitors)
            {
                visitor.LeaveVariableDefinition?.Invoke(node);
            }

            return _;
        }

        public override GraphQLArgument EndVisitArgument(GraphQLArgument argument)
        {
            foreach (var visitor in NodeVisitors)
            {
                visitor.LeaveArgument?.Invoke(argument);
            }

            return base.EndVisitArgument(argument);
        }

        public override GraphQLFieldSelection EndVisitFieldSelection(
            GraphQLFieldSelection selection)
        {
            foreach (var visitor in NodeVisitors)
            {
                visitor.LeaveFieldSelection?.Invoke(selection);
            }

            return base.EndVisitFieldSelection(selection);
        }

        public override GraphQLVariable EndVisitVariable(GraphQLVariable variable)
        {
            foreach (var visitor in NodeVisitors)
            {
                visitor.EnterVariable?.Invoke(variable);
            }

            return base.EndVisitVariable(variable);
        }

        public override GraphQLObjectField BeginVisitObjectField(
            GraphQLObjectField node)
        {
            foreach (var visitor in NodeVisitors)
            {
                visitor.EnterObjectField?.Invoke(node);
            }

            var _ = base.BeginVisitObjectField(node);

            foreach (var visitor in NodeVisitors)
            {
                visitor.LeaveObjectField?.Invoke(node);
            }

            return _;
        }

        public override GraphQLObjectValue BeginVisitObjectValue(
            GraphQLObjectValue node)
        {
            foreach (var visitor in NodeVisitors)
            {
                visitor.EnterObjectValue?.Invoke(node);
            }

            return base.BeginVisitObjectValue(node);
        }

        public override GraphQLObjectValue EndVisitObjectValue(GraphQLObjectValue node)
        {
            foreach (var visitor in NodeVisitors)
            {
                visitor.LeaveObjectValue?.Invoke(node);
            }

            return base.EndVisitObjectValue(node);
        }

        public override ASTNode BeginVisitNode(ASTNode node)
        {
            foreach (var visitor in NodeVisitors)
            {
                visitor.EnterNode?.Invoke(node);
            }

            return base.BeginVisitNode(node);
        }

        public override GraphQLListValue BeginVisitListValue(GraphQLListValue node)
        {
            foreach (var visitor in NodeVisitors)
            {
                visitor.EnterListValue?.Invoke(node);
            }

            return base.BeginVisitListValue(node);
        }

        public override GraphQLListValue EndVisitListValue(GraphQLListValue node)
        {
            foreach (var visitor in NodeVisitors)
            {
                visitor.LeaveListValue?.Invoke(node);
            }

            return base.EndVisitListValue(node);
        }

        private ValidationResult BuildResult()
        {
            return new ValidationResult
            {
                Errors = _errors
            };
        }
    }
}