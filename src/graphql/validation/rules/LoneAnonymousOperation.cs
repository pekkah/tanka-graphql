using System;
using System.Linq;
using GraphQLParser.AST;

namespace tanka.graphql.validation.rules
{
    /// <summary>
    ///     Lone anonymous operation
    ///     A GraphQL document is only valid if when it contains an anonymous operation
    ///     (the query short-hand) that it contains only that one operation definition.
    /// </summary>
    public class LoneAnonymousOperation : IValidationRule
    {
        public Func<string> AnonOperationNotAloneMessage => () =>
            "This anonymous operation must be the only defined operation.";

        public INodeVisitor CreateVisitor(ValidationContext context)
        {
            var operationCount = context.Document.Definitions.OfType<GraphQLOperationDefinition>().Count();

            return new EnterLeaveListener(_ =>
            {
                _.Match<GraphQLOperationDefinition>(op =>
                {
                    if (string.IsNullOrWhiteSpace(op.Name?.Value)
                        && operationCount > 1)
                    {
                        var error = new ValidationError(
                            AnonOperationNotAloneMessage(),
                            op);
                        context.ReportError(error);
                    }
                });
            });
        }
    }
}