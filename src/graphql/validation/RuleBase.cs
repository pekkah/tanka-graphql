using System.Collections.Generic;
using GraphQLParser.AST;

namespace tanka.graphql.validation
{
    public abstract class RuleBase : IRule
    {
        public virtual void BeginVisitAlias(GraphQLName alias, IValidationContext context)
        {
        }

        public virtual void BeginVisitArgument(GraphQLArgument argument,
            IValidationContext context)
        {
        }

        public virtual void BeginVisitArguments(IEnumerable<GraphQLArgument> arguments,
            IValidationContext context)
        {
        }

        public virtual void BeginVisitBooleanValue(GraphQLScalarValue value,
            IValidationContext context)
        {
        }

        public virtual void BeginVisitDirective(GraphQLDirective directive,
            IValidationContext context)
        {
        }

        public virtual void EndVisitDirective(GraphQLDirective directive,
            IValidationContext context)
        {
        }

        public virtual void BeginVisitListValue(GraphQLListValue node,
            IValidationContext context)
        {
        }

        public virtual void EndVisitSelectionSet(GraphQLSelectionSet selectionSet,
            IValidationContext context)
        {
        }

        public virtual void EndVisitVariableDefinition(GraphQLVariableDefinition node,
            IValidationContext context)
        {
        }

        public virtual void EndVisitObjectField(GraphQLObjectField node, IValidationContext context)
        {
        }

        public virtual void EndVisitEnumValue(GraphQLScalarValue value, IValidationContext context)
        {
        }

        public virtual void BeginVisitDirectives(IEnumerable<GraphQLDirective> directives,
            IValidationContext context)
        {
        }

        public virtual void BeginVisitEnumValue(GraphQLScalarValue value,
            IValidationContext context)
        {
        }

        public virtual void BeginVisitFieldSelection(GraphQLFieldSelection selection,
            IValidationContext context)
        {
        }

        public virtual void BeginVisitFloatValue(GraphQLScalarValue value,
            IValidationContext context)
        {
        }

        public virtual void BeginVisitFragmentDefinition(GraphQLFragmentDefinition node,
            IValidationContext context)
        {
        }

        public virtual void EndVisitFragmentDefinition(GraphQLFragmentDefinition node,
            IValidationContext context)
        {
        }

        public virtual void BeginVisitFragmentSpread(GraphQLFragmentSpread fragmentSpread,
            IValidationContext context)
        {
        }

        public virtual void BeginVisitInlineFragment(GraphQLInlineFragment inlineFragment,
            IValidationContext context)
        {
        }

        public virtual void EndVisitInlineFragment(GraphQLInlineFragment inlineFragment,
            IValidationContext context)
        {
        }

        public virtual void BeginVisitIntValue(GraphQLScalarValue value,
            IValidationContext context)
        {
        }

        public virtual void BeginVisitName(GraphQLName name, IValidationContext context)
        {
        }

        public virtual void BeginVisitNamedType(GraphQLNamedType typeCondition,
            IValidationContext context)
        {
        }

        public virtual void BeginVisitNode(ASTNode node, IValidationContext context)
        {
        }

        public virtual void BeginVisitOperationDefinition(GraphQLOperationDefinition definition,
            IValidationContext context)
        {
        }

        public virtual void EndVisitOperationDefinition(GraphQLOperationDefinition definition,
            IValidationContext context)
        {
        }

        public virtual void BeginVisitSelectionSet(GraphQLSelectionSet selectionSet,
            IValidationContext context)
        {
        }

        public virtual void BeginVisitStringValue(GraphQLScalarValue value,
            IValidationContext context)
        {
        }

        public virtual void BeginVisitVariable(GraphQLVariable variable,
            IValidationContext context)
        {
        }

        public virtual void BeginVisitVariableDefinition(GraphQLVariableDefinition node,
            IValidationContext context)
        {
        }

        public virtual void BeginVisitVariableDefinitions(
            IEnumerable<GraphQLVariableDefinition> variableDefinitions, IValidationContext context)
        {
        }

        public virtual void EndVisitArgument(GraphQLArgument argument,
            IValidationContext context)
        {
        }

        public virtual void EndVisitFieldSelection(GraphQLFieldSelection selection,
            IValidationContext context)
        {
        }

        public virtual void EndVisitVariable(GraphQLVariable variable,
            IValidationContext context)
        {
        }

        public virtual void Visit(GraphQLDocument document, IValidationContext context)
        {
        }

        public virtual void BeginVisitObjectField(GraphQLObjectField node,
            IValidationContext context)
        {
        }

        public virtual void BeginVisitObjectValue(GraphQLObjectValue node,
            IValidationContext context)
        {
        }

        public virtual void EndVisitObjectValue(GraphQLObjectValue node,
            IValidationContext context)
        {
        }

        public virtual void EndVisitListValue(GraphQLListValue node, IValidationContext context)
        {
        }

        public abstract IEnumerable<ASTNodeKind> AppliesToNodeKinds { get; }
    }
}