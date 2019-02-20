using System.Collections.Generic;
using GraphQLParser.AST;

namespace tanka.graphql.validation
{
    public interface IDocumentRuleVisitor
    {
        IEnumerable<ValidationError> BeginVisitAlias(GraphQLName alias);

        IEnumerable<ValidationError> BeginVisitArgument(GraphQLArgument argument);

        IEnumerable<ValidationError> BeginVisitArguments(
            IEnumerable<GraphQLArgument> arguments);

        IEnumerable<ValidationError> BeginVisitBooleanValue(
            GraphQLScalarValue value);

        IEnumerable<ValidationError> BeginVisitDirective(GraphQLDirective directive);

        IEnumerable<ValidationError> BeginVisitDirectives(
            IEnumerable<GraphQLDirective> directives);

        IEnumerable<ValidationError> BeginVisitEnumValue(GraphQLScalarValue value);

        IEnumerable<ValidationError> BeginVisitFieldSelection(
            GraphQLFieldSelection selection);

        IEnumerable<ValidationError> BeginVisitFloatValue(
            GraphQLScalarValue value);

        IEnumerable<ValidationError> BeginVisitFragmentDefinition(
            GraphQLFragmentDefinition node);

        IEnumerable<ValidationError> BeginVisitFragmentSpread(
            GraphQLFragmentSpread fragmentSpread);

        IEnumerable<ValidationError> BeginVisitInlineFragment(
            GraphQLInlineFragment inlineFragment);

        IEnumerable<ValidationError> BeginVisitIntValue(GraphQLScalarValue value);

        IEnumerable<ValidationError> BeginVisitName(GraphQLName name);

        IEnumerable<ValidationError> BeginVisitNamedType(
            GraphQLNamedType typeCondition);

        IEnumerable<ValidationError> BeginVisitNode(ASTNode node);

        IEnumerable<ValidationError> BeginVisitOperationDefinition(
            GraphQLOperationDefinition definition);

        IEnumerable<ValidationError> EndVisitOperationDefinition(
            GraphQLOperationDefinition definition);

        IEnumerable<ValidationError> BeginVisitSelectionSet(
            GraphQLSelectionSet selectionSet);

        IEnumerable<ValidationError> BeginVisitStringValue(
            GraphQLScalarValue value);

        IEnumerable<ValidationError> BeginVisitVariable(GraphQLVariable variable);

        IEnumerable<ValidationError> BeginVisitVariableDefinition(
            GraphQLVariableDefinition node);

        IEnumerable<ValidationError> BeginVisitVariableDefinitions(
            IEnumerable<GraphQLVariableDefinition> variableDefinitions);

        IEnumerable<ValidationError> EndVisitArgument(GraphQLArgument argument);

        IEnumerable<ValidationError> EndVisitFieldSelection(
            GraphQLFieldSelection selection);

        IEnumerable<ValidationError> EndVisitVariable(GraphQLVariable variable);

        IEnumerable<ValidationError> Visit(GraphQLDocument document);

        IEnumerable<ValidationError> BeginVisitObjectField(
            GraphQLObjectField node);

        IEnumerable<ValidationError> BeginVisitObjectValue(
            GraphQLObjectValue node);

        IEnumerable<ValidationError> EndVisitObjectValue(GraphQLObjectValue node);

        IEnumerable<ValidationError> EndVisitListValue(GraphQLListValue node);

    }
}