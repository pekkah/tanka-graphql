using System.Collections.Generic;
using GraphQLParser.AST;

namespace tanka.graphql.validation
{
    public interface IDocumentRuleVisitor
    {
        IEnumerable<ValidationError> BeginVisitAlias(GraphQLName alias,
            IValidationContext context);

        IEnumerable<ValidationError> BeginVisitArgument(GraphQLArgument argument,
            IValidationContext context);

        IEnumerable<ValidationError> BeginVisitArguments(
            IEnumerable<GraphQLArgument> arguments,
            IValidationContext context);

        IEnumerable<ValidationError> BeginVisitBooleanValue(
            GraphQLScalarValue value, IValidationContext context);

        IEnumerable<ValidationError> BeginVisitDirective(GraphQLDirective directive,
            IValidationContext context);

        IEnumerable<ValidationError> BeginVisitDirectives(
            IEnumerable<GraphQLDirective> directives, IValidationContext context);

        IEnumerable<ValidationError> BeginVisitEnumValue(GraphQLScalarValue value,
            IValidationContext context);

        IEnumerable<ValidationError> BeginVisitFieldSelection(
            GraphQLFieldSelection selection, IValidationContext context);

        IEnumerable<ValidationError> BeginVisitFloatValue(
            GraphQLScalarValue value, IValidationContext context);

        IEnumerable<ValidationError> BeginVisitFragmentDefinition(
            GraphQLFragmentDefinition node, IValidationContext context);

        IEnumerable<ValidationError> BeginVisitFragmentSpread(
            GraphQLFragmentSpread fragmentSpread, IValidationContext context);

        IEnumerable<ValidationError> BeginVisitInlineFragment(
            GraphQLInlineFragment inlineFragment, IValidationContext context);

        IEnumerable<ValidationError> BeginVisitIntValue(GraphQLScalarValue value,
            IValidationContext context);

        IEnumerable<ValidationError> BeginVisitName(GraphQLName name,
            IValidationContext context);

        IEnumerable<ValidationError> BeginVisitNamedType(
            GraphQLNamedType typeCondition, IValidationContext context);

        IEnumerable<ValidationError> BeginVisitNode(ASTNode node,
            IValidationContext context);

        IEnumerable<ValidationError> BeginVisitOperationDefinition(
            GraphQLOperationDefinition definition, IValidationContext context);

        IEnumerable<ValidationError> EndVisitOperationDefinition(
            GraphQLOperationDefinition definition, IValidationContext context);

        IEnumerable<ValidationError> BeginVisitSelectionSet(
            GraphQLSelectionSet selectionSet, IValidationContext context);

        IEnumerable<ValidationError> BeginVisitStringValue(
            GraphQLScalarValue value, IValidationContext context);

        IEnumerable<ValidationError> BeginVisitVariable(GraphQLVariable variable,
            IValidationContext context);

        IEnumerable<ValidationError> BeginVisitVariableDefinition(
            GraphQLVariableDefinition node, IValidationContext context);

        IEnumerable<ValidationError> BeginVisitVariableDefinitions(
            IEnumerable<GraphQLVariableDefinition> variableDefinitions,
            IValidationContext context);

        IEnumerable<ValidationError> EndVisitArgument(GraphQLArgument argument,
            IValidationContext context);

        IEnumerable<ValidationError> EndVisitFieldSelection(
            GraphQLFieldSelection selection, IValidationContext context);

        IEnumerable<ValidationError> EndVisitVariable(GraphQLVariable variable,
            IValidationContext context);

        IEnumerable<ValidationError> Visit(GraphQLDocument document,
            IValidationContext context);

        IEnumerable<ValidationError> BeginVisitObjectField(
            GraphQLObjectField node, IValidationContext context);

        IEnumerable<ValidationError> BeginVisitObjectValue(
            GraphQLObjectValue node, IValidationContext context);

        IEnumerable<ValidationError> EndVisitObjectValue(GraphQLObjectValue node,
            IValidationContext context);

        IEnumerable<ValidationError> EndVisitListValue(GraphQLListValue node,
            IValidationContext context);

        IEnumerable<ValidationError> EndVisitFragmentDefinition(GraphQLFragmentDefinition node,
            IValidationContext context);

        IEnumerable<ValidationError> EndVisitInlineFragment(GraphQLInlineFragment inlineFragment, IValidationContext context);

        IEnumerable<ValidationError> EndVisitDirective(GraphQLDirective directive,
            IValidationContext context);

        IEnumerable<ValidationError> BeginVisitListValue(GraphQLListValue node, IValidationContext context);
        IEnumerable<ValidationError> EndVisitSelectionSet(GraphQLSelectionSet selectionSet, IValidationContext context);
        IEnumerable<ValidationError> EndVisitVariableDefinition(GraphQLVariableDefinition node, IValidationContext context);
        IEnumerable<ValidationError> EndVisitObjectField(GraphQLObjectField node, IValidationContext context);
        IEnumerable<ValidationError> EndVisitEnumValue(GraphQLScalarValue value, IValidationContext context);
    }
}