using System.Collections.Generic;
using System.Linq;
using GraphQLParser.AST;
using tanka.graphql.execution;
using tanka.graphql.type;

namespace tanka.graphql.validation.rules2
{
    /// <summary>
    /// For each subscription operation definition subscription in the document
    /// Let subscriptionType be the root Subscription type in schema.
    /// Let selectionSet be the top level selection set on subscription.
    /// Let variableValues be the empty set.
    /// Let groupedFieldSet be the result of CollectFields(subscriptionType, selectionSet, variableValues).
    /// groupedFieldSet must have exactly one entry.
    /// </summary>
    public class R5231SingleRootField : Rule
    {
        public override IEnumerable<ASTNodeKind> AppliesToNodeKinds
            => new[]
            {
                ASTNodeKind.Document
            };

        public override IEnumerable<ValidationError> Visit(GraphQLDocument document, IValidationContext context)
        {
            var subscriptions = document.Definitions
                .OfType<GraphQLOperationDefinition>()
                .Where(op => op.Operation == OperationType.Subscription)
                .ToList();

            if (!subscriptions.Any())
                yield break;

            var schema = context.Schema;
            //todo(pekka): should this report error?
            if (schema.Subscription == null)
                yield break;

            var subscriptionType = schema.Subscription;
            foreach (var subscription in subscriptions)
            {
                var selectionSet = subscription.SelectionSet;
                var variableValues = new Dictionary<string, object>();

                var groupedFieldSet = SelectionSets.CollectFields(
                    schema,
                    context.Document,
                    subscriptionType,
                    selectionSet,
                    variableValues);

                if (groupedFieldSet.Count != 1)
                    yield return new ValidationError(
                        Errors.R5231SingleRootField,
                        "Subscription operations must have exactly one root field.",
                        subscription);
            }
        }
    }
}