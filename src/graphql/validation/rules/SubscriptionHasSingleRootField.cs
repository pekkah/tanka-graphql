using System.Collections.Generic;
using tanka.graphql.execution;
using GraphQLParser.AST;

namespace tanka.graphql.validation.rules
{
    public class SubscriptionHasSingleRootField : IValidationRule
    {
        public INodeVisitor CreateVisitor(ValidationContext context)
        {
            return new EnterLeaveListener(
                _ => { _.Match<GraphQLOperationDefinition>(node => Validate(context, node)); });
        }

        private void Validate(ValidationContext context, GraphQLOperationDefinition node)
        {
            if (node.Operation != OperationType.Subscription)
                return;

            var subscriptionType = context.Schema.Subscription;
            var selectionSet = node.SelectionSet;
            var variableValues = new Dictionary<string, object>();

            var groupedFieldSet = SelectionSets.CollectFields(
                context.Schema,
                context.Document,
                subscriptionType,
                selectionSet,
                variableValues);

            if (groupedFieldSet.Count != 1)
                context.ReportError(new ValidationError(
                    "Subscription operations must have exactly one root field.",
                    node));
        }
    }
}