using System.Linq;
using fugu.graphql.type;
using GraphQLParser.AST;

namespace fugu.graphql.validation.rules
{
    /// <summary>
    /// Provided required arguments
    ///
    /// A field or directive is only valid if all required (non-null) field arguments
    /// have been provided.
    /// </summary>
    public class ProvidedNonNullArguments : IValidationRule
    {
        public string MissingFieldArgMessage(string fieldName, string argName, string type)
        {
            return $"Field \"{fieldName}\" argument \"{argName}\" of type \"{type}\" is required but not provided.";
        }

        public string MissingDirectiveArgMessage(string directiveName, string argName, string type)
        {
            return $"Directive \"{directiveName}\" argument \"{argName}\" of type \"{type}\" is required but not provided.";
        }

        public INodeVisitor CreateVisitor(ValidationContext context)
        {
            return new EnterLeaveListener(_ =>
            {
                _.Match<GraphQLFieldSelection>(leave: node =>
                {
                    var fieldDef = context.TypeInfo.GetFieldDef();

                    if (fieldDef == null)
                    {
                        return;
                    }

                    fieldDef.Arguments?.ToList().ForEach(arg =>
                    {
                        var type = arg.Value.Type;
                        var argAst = node.Arguments?.SingleOrDefault(a => a.Name.Value == arg.Key);

                        if (argAst == null && type is NonNull)
                        {
                            context.ReportError(
                                new ValidationError(
                                    MissingFieldArgMessage(node.Name.Value, arg.Key, type.Name),
                                    node));
                        }
                    });
                });

                _.Match<GraphQLDirective>(leave: node =>
                {
                    var directive = context.TypeInfo.GetDirective();

                    if (directive == null)
                    {
                        return;
                    }

                    directive.Arguments?.ToList().ForEach(arg =>
                    {
                        var type = arg.Value.Type;
                        var argAst = node.Arguments?.SingleOrDefault(a => a.Name.Value == arg.Key);

                        if (argAst == null && type is NonNull)
                        {
                            context.ReportError(
                                new ValidationError(
                                    MissingDirectiveArgMessage(node.Name.Value, arg.Key, type.Name),
                                    node));
                        }
                    });
                });
            });
        }
    }
}
