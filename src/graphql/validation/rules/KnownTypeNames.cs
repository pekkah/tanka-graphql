using System.Linq;
using tanka.graphql.type;
using GraphQLParser.AST;

namespace tanka.graphql.validation.rules
{
    /// <summary>
    ///     Known type names
    ///     A GraphQL document is only valid if referenced types (specifically
    ///     variable definitions and fragment conditions) are defined by the type schema.
    /// </summary>
    public class KnownTypeNames : IValidationRule
    {
        public INodeVisitor CreateVisitor(ValidationContext context)
        {
            return new EnterLeaveListener(_ =>
            {
                _.Match<GraphQLNamedType>(leave: node =>
                {
                    var type = context.Schema.GetNamedType(node.Name.Value);
                    if (type == null)
                    {
                        var typeNames = context.Schema.QueryTypes<INamedType>().Select(x => x.Name).ToArray();
                        var suggestionList = Enumerable.Empty<string>().ToArray();
                        context.ReportError(new ValidationError(UnknownTypeMessage(node.Name.Value, suggestionList),
                            node));
                    }
                });
            });
        }

        public string UnknownTypeMessage(string type, string[] suggestedTypes)
        {
            var message = $"Unknown type {type}.";
            if (suggestedTypes != null && suggestedTypes.Length > 0)
                message += $" Did you mean {string.Join(",", suggestedTypes)}?";
            return message;
        }
    }
}