using System;
using System.Linq;
using GraphQLParser.AST;

namespace fugu.graphql.validation.rules
{
    /// <summary>
    ///     Known argument names
    ///     A GraphQL field is only valid if all supplied arguments are defined by
    ///     that field.
    /// </summary>
    public class KnownArgumentNames : IValidationRule
    {
        public INodeVisitor CreateVisitor(ValidationContext context)
        {
            return new EnterLeaveListener(_ =>
            {
                _.Match<GraphQLArgument>(node =>
                {
                    var ancestors = context.TypeInfo.GetAncestors();
                    var argumentOf = ancestors[ancestors.Length - 2];
                    if (argumentOf is GraphQLFieldSelection)
                    {
                        var fieldDef = context.TypeInfo.GetFieldDef();
                        if (fieldDef != null)
                        {
                            var fieldArgDef = fieldDef.Arguments?.SingleOrDefault(a => a.Key == node.Name.Value);
                            if (fieldArgDef == null)
                            {
                                var parentType = context.TypeInfo.GetParentType()
                                                 ?? throw new ArgumentNullException(
                                                     nameof(context.TypeInfo.GetParentType));

                                context.ReportError(new ValidationError(
                                    UnknownArgMessage(
                                        node.Name.Value,
                                        fieldDef.ToString(),
                                        parentType.Name,
                                        null),
                                    node));
                            }
                        }
                    }

                    /*else if (argumentOf is Directive)
                    {
                        var directive = context.TypeInfo.GetDirective();
                        if (directive != null)
                        {
                            var directiveArgDef = directive.Arguments?.Find(node.Name);
                            if (directiveArgDef == null)
                            {
                                context.ReportError(new ValidationError(
                                    context.OriginalQuery,
                                    "5.3.1",
                                    UnknownDirectiveArgMessage(
                                        node.Name,
                                        directive.Name,
                                        StringUtils.SuggestionList(node.Name, directive.Arguments?.Select(q => q.Name))),
                                    node));
                            }
                        }
                    }*/
                });
            });
        }

        public string UnknownArgMessage(string argName, string fieldName, string type, string[] suggestedArgs)
        {
            var message = $"Unknown argument \"{argName}\" on field \"{fieldName}\" of type \"{type}\".";
            if (suggestedArgs != null && suggestedArgs.Length > 0)
                message += $"Did you mean {string.Join(",", suggestedArgs)}";
            return message;
        }

        public string UnknownDirectiveArgMessage(string argName, string directiveName, string[] suggestedArgs)
        {
            var message = $"Unknown argument \"{argName}\" on directive \"{directiveName}\".";
            if (suggestedArgs != null && suggestedArgs.Length > 0)
                message += $"Did you mean {string.Join(",", suggestedArgs)}";
            return message;
        }
    }
}