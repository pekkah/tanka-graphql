using System.Collections.Generic;
using GraphQLParser.AST;
using tanka.graphql.type;

namespace tanka.graphql.validation.rules2
{
    /// <summary>
    ///     For each selection in the document.
    ///     Let fieldName be the target field of selection
    ///     fieldName must be defined on type in scope
    /// </summary>
    public class R531FieldSelections : Rule
    {
        public override IEnumerable<ASTNodeKind> AppliesToNodeKinds => new[]
        {
            ASTNodeKind.OperationDefinition,
            ASTNodeKind.InlineFragment,
            ASTNodeKind.FragmentDefinition,
            ASTNodeKind.Field
        };

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

            var field = GetField(fieldName, context.Schema);

            if (field == null)
            {
                yield return new ValidationError(
                    Errors.R531FieldSelections,
                    "The target field of a field selection must be defined " +
                    "on the scoped type of the selection set. There are no " +
                    "limitations on alias names.",
                    selection);
            }
            else
            {
                if (field.Type is ComplexType complexType)
                    ParentType = complexType;
            }
        }

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