using System.Collections.Generic;
using GraphQLParser.AST;

namespace tanka.graphql.validation
{
    public interface IRule
    {
        IEnumerable<ASTNodeKind> AppliesToNodeKinds { get; }

        void BeginVisitAlias(GraphQLName alias,
            IValidationContext context);

        void BeginVisitArgument(GraphQLArgument argument,
            IValidationContext context);

        void BeginVisitArguments(
            IEnumerable<GraphQLArgument> arguments,
            IValidationContext context);

        void BeginVisitBooleanValue(
            GraphQLScalarValue value, IValidationContext context);

        void BeginVisitDirective(GraphQLDirective directive,
            IValidationContext context);

        void BeginVisitDirectives(
            IEnumerable<GraphQLDirective> directives, IValidationContext context);

        void BeginVisitEnumValue(GraphQLScalarValue value,
            IValidationContext context);

        void BeginVisitFieldSelection(
            GraphQLFieldSelection selection, IValidationContext context);

        void BeginVisitFloatValue(
            GraphQLScalarValue value, IValidationContext context);

        void BeginVisitFragmentDefinition(
            GraphQLFragmentDefinition node, IValidationContext context);

        void BeginVisitFragmentSpread(
            GraphQLFragmentSpread fragmentSpread, IValidationContext context);

        void BeginVisitInlineFragment(
            GraphQLInlineFragment inlineFragment, IValidationContext context);

        void BeginVisitIntValue(GraphQLScalarValue value,
            IValidationContext context);

        void BeginVisitName(GraphQLName name,
            IValidationContext context);

        void BeginVisitNamedType(
            GraphQLNamedType typeCondition, IValidationContext context);

        void BeginVisitNode(ASTNode node,
            IValidationContext context);

        void BeginVisitOperationDefinition(
            GraphQLOperationDefinition definition, IValidationContext context);

        void EndVisitOperationDefinition(
            GraphQLOperationDefinition definition, IValidationContext context);

        void BeginVisitSelectionSet(
            GraphQLSelectionSet selectionSet, IValidationContext context);

        void BeginVisitStringValue(
            GraphQLScalarValue value, IValidationContext context);

        void BeginVisitVariable(GraphQLVariable variable,
            IValidationContext context);

        void BeginVisitVariableDefinition(
            GraphQLVariableDefinition node, IValidationContext context);

        void BeginVisitVariableDefinitions(
            IEnumerable<GraphQLVariableDefinition> variableDefinitions,
            IValidationContext context);

        void EndVisitArgument(GraphQLArgument argument,
            IValidationContext context);

        void EndVisitFieldSelection(
            GraphQLFieldSelection selection, IValidationContext context);

        void EndVisitVariable(GraphQLVariable variable,
            IValidationContext context);

        void Visit(GraphQLDocument document,
            IValidationContext context);

        void BeginVisitObjectField(
            GraphQLObjectField node, IValidationContext context);

        void BeginVisitObjectValue(
            GraphQLObjectValue node, IValidationContext context);

        void EndVisitObjectValue(GraphQLObjectValue node,
            IValidationContext context);

        void EndVisitListValue(GraphQLListValue node,
            IValidationContext context);

        void EndVisitFragmentDefinition(GraphQLFragmentDefinition node,
            IValidationContext context);

        void EndVisitInlineFragment(GraphQLInlineFragment inlineFragment, IValidationContext context);

        void EndVisitDirective(GraphQLDirective directive,
            IValidationContext context);

        void BeginVisitListValue(GraphQLListValue node, IValidationContext context);
        
        void EndVisitSelectionSet(GraphQLSelectionSet selectionSet, IValidationContext context);
        
        void EndVisitVariableDefinition(GraphQLVariableDefinition node, IValidationContext context);
        
        void EndVisitObjectField(GraphQLObjectField node, IValidationContext context);

        void EndVisitEnumValue(GraphQLScalarValue value, IValidationContext context);

        void EndVisit(GraphQLDocument document, IValidationContext context);
    }
}