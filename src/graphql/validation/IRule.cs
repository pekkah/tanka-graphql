using System.Collections.Generic;
using System.Linq;
using GraphQLParser.AST;
using tanka.graphql.type;

namespace tanka.graphql.validation
{
    public interface IRule : IDocumentRuleVisitor
    {
        IEnumerable<ASTNodeKind> AppliesToNodeKinds { get; }
    }

    public abstract class Rule : IRule
    {
        public virtual IEnumerable<ValidationError> BeginVisitAlias(GraphQLName alias, IValidationContext context)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitArgument(GraphQLArgument argument, IValidationContext context)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitArguments(IEnumerable<GraphQLArgument> arguments, IValidationContext context)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitBooleanValue(GraphQLScalarValue value, IValidationContext context)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitDirective(GraphQLDirective directive, IValidationContext context)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitDirectives(IEnumerable<GraphQLDirective> directives, IValidationContext context)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitEnumValue(GraphQLScalarValue value, IValidationContext context)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitFieldSelection(GraphQLFieldSelection selection, IValidationContext context)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitFloatValue(GraphQLScalarValue value, IValidationContext context)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitFragmentDefinition(GraphQLFragmentDefinition node, IValidationContext context)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitFragmentSpread(GraphQLFragmentSpread fragmentSpread, IValidationContext context)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitInlineFragment(GraphQLInlineFragment inlineFragment, IValidationContext context)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitIntValue(GraphQLScalarValue value, IValidationContext context)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitName(GraphQLName name, IValidationContext context)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitNamedType(GraphQLNamedType typeCondition, IValidationContext context)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitNode(ASTNode node, IValidationContext context)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitOperationDefinition(GraphQLOperationDefinition definition, IValidationContext context)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> EndVisitOperationDefinition(GraphQLOperationDefinition definition, IValidationContext context)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitSelectionSet(GraphQLSelectionSet selectionSet, IValidationContext context)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitStringValue(GraphQLScalarValue value, IValidationContext context)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitVariable(GraphQLVariable variable, IValidationContext context)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitVariableDefinition(GraphQLVariableDefinition node, IValidationContext context)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitVariableDefinitions(
            IEnumerable<GraphQLVariableDefinition> variableDefinitions, IValidationContext context)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> EndVisitArgument(GraphQLArgument argument, IValidationContext context)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> EndVisitFieldSelection(GraphQLFieldSelection selection, IValidationContext context)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> EndVisitVariable(GraphQLVariable variable, IValidationContext context)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> Visit(GraphQLDocument document, IValidationContext context)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitObjectField(GraphQLObjectField node, IValidationContext context)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitObjectValue(GraphQLObjectValue node, IValidationContext context)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> EndVisitObjectValue(GraphQLObjectValue node, IValidationContext context)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> EndVisitListValue(GraphQLListValue node, IValidationContext context)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public abstract IEnumerable<ASTNodeKind> AppliesToNodeKinds { get; }
    }
}