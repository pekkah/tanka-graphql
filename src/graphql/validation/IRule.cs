using System.Collections.Generic;
using System.Linq;
using GraphQLParser.AST;

namespace tanka.graphql.validation
{
    public interface IRule : IDocumentRuleVisitor
    {
        IEnumerable<ASTNodeKind> AppliesToNodeKinds { get; }
    }

    public abstract class Rule : IRule
    {
        public virtual IEnumerable<ValidationError> BeginVisitAlias(GraphQLName alias)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitArgument(GraphQLArgument argument)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitArguments(IEnumerable<GraphQLArgument> arguments)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitBooleanValue(GraphQLScalarValue value)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitDirective(GraphQLDirective directive)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitDirectives(IEnumerable<GraphQLDirective> directives)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitEnumValue(GraphQLScalarValue value)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitFieldSelection(GraphQLFieldSelection selection)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitFloatValue(GraphQLScalarValue value)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitFragmentDefinition(GraphQLFragmentDefinition node)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitFragmentSpread(GraphQLFragmentSpread fragmentSpread)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitInlineFragment(GraphQLInlineFragment inlineFragment)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitIntValue(GraphQLScalarValue value)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitName(GraphQLName name)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitNamedType(GraphQLNamedType typeCondition)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitNode(ASTNode node)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitOperationDefinition(GraphQLOperationDefinition definition)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> EndVisitOperationDefinition(GraphQLOperationDefinition definition)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitSelectionSet(GraphQLSelectionSet selectionSet)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitStringValue(GraphQLScalarValue value)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitVariable(GraphQLVariable variable)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitVariableDefinition(GraphQLVariableDefinition node)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitVariableDefinitions(
            IEnumerable<GraphQLVariableDefinition> variableDefinitions)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> EndVisitArgument(GraphQLArgument argument)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> EndVisitFieldSelection(GraphQLFieldSelection selection)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> EndVisitVariable(GraphQLVariable variable)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> Visit(GraphQLDocument document)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitObjectField(GraphQLObjectField node)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> BeginVisitObjectValue(GraphQLObjectValue node)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> EndVisitObjectValue(GraphQLObjectValue node)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public virtual IEnumerable<ValidationError> EndVisitListValue(GraphQLListValue node)
        {
            return Enumerable.Empty<ValidationError>();
        }

        public abstract IEnumerable<ASTNodeKind> AppliesToNodeKinds { get; }
    }
}