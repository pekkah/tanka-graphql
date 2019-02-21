using System.Collections.Generic;
using System.Linq;
using GraphQLParser.AST;
using tanka.graphql.type;

namespace tanka.graphql.validation.rules2
{
    /// <summary>
    ///     For each argument in the document
    ///     Let argumentName be the Name of argument.
    ///     Let argumentDefinition be the argument definition provided by the parent field or definition named argumentName.
    ///     argumentDefinition must exist.
    /// </summary>
    public class R541ArgumentNames : Rule
    {
        public override IEnumerable<ASTNodeKind> AppliesToNodeKinds => new[]
        {
            ASTNodeKind.OperationDefinition,
            ASTNodeKind.InlineFragment,
            ASTNodeKind.FragmentDefinition,
            ASTNodeKind.Field,
            ASTNodeKind.Directive,
            ASTNodeKind.Argument
        };

        public override IEnumerable<ValidationError> BeginVisitArgument(
            GraphQLArgument argument,
            IValidationContext context)
        {
            var argumentName = argument.Name.Value;

            Argument argumentDefinition = null;

            if (ParentDirectiveType != null)
            {
                argumentDefinition = ParentDirectiveType.GetArgument(argumentName);
            }
            else
            {
                argumentDefinition = ParentField?.GetArgument(argumentName);
            }

            if (argumentDefinition == null)
            {
                yield return new ValidationError(
                    Errors.R541ArgumentNames,
                    "Every argument provided to a field or directive " +
                    "must be defined in the set of possible arguments of that " +
                    "field or directive.",
                    argument);
            }
        }

        public INamedType ParentType { get; set; }

        public override IEnumerable<ValidationError> BeginVisitInlineFragment(GraphQLInlineFragment inlineFragment,
            IValidationContext context)
        {
            var typeName = inlineFragment.TypeCondition.Name.Value;
            ParentType = context.Schema.GetNamedType(typeName);
            yield break;
        }

        public override IEnumerable<ValidationError> BeginVisitFragmentDefinition(GraphQLFragmentDefinition node,
            IValidationContext context)
        {
            var typeName = node.TypeCondition.Name.Value;
            ParentType = context.Schema.GetNamedType(typeName);
            yield break;
        }

        public override IEnumerable<ValidationError> BeginVisitOperationDefinition(
            GraphQLOperationDefinition definition, IValidationContext context)
        {
            var schema = context.Schema;

            switch (definition.Operation)
            {
                case OperationType.Query:
                    ParentType = schema.Query;
                    break;
                case OperationType.Mutation:
                    ParentType = schema.Mutation;
                    break;
                case OperationType.Subscription:
                    ParentType = schema.Subscription;
                    break;
            }

            yield break;
        }

        public override IEnumerable<ValidationError> BeginVisitFieldSelection(GraphQLFieldSelection selection,
            IValidationContext context)
        {
            var fieldName = selection.Name.Value;

            if (fieldName == "__typename")
                yield break;

            ParentField = GetField(fieldName, context.Schema);
        }

        public override IEnumerable<ValidationError> BeginVisitDirective(
            GraphQLDirective directive, 
            IValidationContext context)
        {
            ParentDirectiveType = context.Schema.GetDirective(directive.Name.Value);
            yield break;
        }

        public DirectiveType ParentDirectiveType { get; set; }

        public IField ParentField { get; set; }

        private IField GetField(string fieldName, ISchema schema)
        {
            if (ParentType is null)
                return null;

            if (!(ParentType is ComplexType))
                return null;

            return schema.GetField(ParentType.Name, fieldName);
        }
    }
}