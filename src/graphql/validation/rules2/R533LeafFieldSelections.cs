using System.Collections.Generic;
using System.Linq;
using GraphQLParser.AST;
using tanka.graphql.type;

namespace tanka.graphql.validation.rules2
{
    /// <summary>
    ///     For each selection in the document
    ///     Let selectionType be the result type of selection
    ///     If selectionType is a scalar or enum:
    ///     The subselection set of that selection must be empty
    ///     If selectionType is an interface, union, or object
    ///     The subselection set of that selection must NOT BE empty
    /// </summary>
    public class R533LeafFieldSelections : Rule
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

            if (field != null)
            {
                var selectionType = field.Type;
                var hasSubSelection = selection.SelectionSet?.Selections?.Any();

                if (selectionType is ScalarType && hasSubSelection == true)
                {
                    yield return new ValidationError(
                        Errors.R533LeafFieldSelections,
                        "Field selections on scalars or enums are never " +
                        "allowed, because they are the leaf nodes of any GraphQL query.",
                        selection);
                }

                if (selectionType is EnumType && hasSubSelection == true)
                {
                    yield return new ValidationError(
                        Errors.R533LeafFieldSelections,
                        "Field selections on scalars or enums are never " +
                        "allowed, because they are the leaf nodes of any GraphQL query.",
                        selection);
                }

                if (selectionType is ComplexType && hasSubSelection == null)
                {
                    yield return new ValidationError(
                        Errors.R533LeafFieldSelections,
                        "Leaf selections on objects, interfaces, and unions " +
                        "without subfields are disallowed.",
                        selection);
                }

                if (selectionType is UnionType && hasSubSelection == null)
                {
                    yield return new ValidationError(
                        Errors.R533LeafFieldSelections,
                        "Leaf selections on objects, interfaces, and unions " +
                        "without subfields are disallowed.",
                        selection);
                }
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