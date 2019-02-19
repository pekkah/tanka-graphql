using System;
using System.Linq;
using tanka.graphql.type;
using tanka.graphql.type.converters;
using GraphQLParser.AST;

namespace tanka.graphql.validation.rules
{
    /// <summary>
    ///     Scalar leafs
    ///     A GraphQL document is valid only if all leaf fields (fields without
    ///     sub selections) are of scalar or enum types.
    /// </summary>
    public class ScalarLeafs : IValidationRule
    {
        public Func<string, string, string> NoSubselectionAllowedMessage = (field, type) =>
            $"Field {field} of type {type} must not have a sub selection";

        public Func<string, string, string> RequiredSubselectionMessage = (field, type) =>
            $"Field {field} of type {type} must have a sub selection";

        public INodeVisitor CreateVisitor(ValidationContext context)
        {
            return new EnterLeaveListener(_ =>
            {
                _.Match<GraphQLFieldSelection>(f => Field(context.TypeInfo.GetLastType()?.Unwrap(), f, context));
            });
        }

        private void Field(IType type, GraphQLFieldSelection field, ValidationContext context)
        {
            if (type == null) return;

            if (type is IValueConverter)
            {
                if (field.SelectionSet != null && field.SelectionSet.Selections.Any())
                {
                    var error = new ValidationError(NoSubselectionAllowedMessage(field.Name.Value, type?.ToString()),
                        field.SelectionSet);
                    context.ReportError(error);
                }
            }
            else if (field.SelectionSet == null || !field.SelectionSet.Selections.Any())
            {
                var error = new ValidationError(RequiredSubselectionMessage(field.Name.Value, type?.ToString()), field);
                context.ReportError(error);
            }
        }
    }
}