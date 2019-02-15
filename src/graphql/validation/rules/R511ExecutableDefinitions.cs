using GraphQLParser.AST;

namespace tanka.graphql.validation.rules
{
    public class R511ExecutableDefinitions : IValidationRule
    {
        public INodeVisitor CreateVisitor(ValidationContext context)
        {
            return new EnterLeaveListener(_ =>
            {
                _.Match<GraphQLDocument>(
                    document =>
                    {
                        foreach (var definition in document.Definitions)
                        {
                            var valid = definition.Kind == ASTNodeKind.OperationDefinition
                                        || definition.Kind == ASTNodeKind.FragmentDefinition;

                            if (!valid)
                                context.ReportError(new ValidationError(
                                    Errors.R511ExecutableDefinitions,
                                    "GraphQL execution will only consider the " +
                                    "executable definitions Operation and Fragment. " +
                                    "Type system definitions and extensions are not " +
                                    "executable, and are not considered during execution.",
                                    definition));
                        }
                    });
            });
        }
    }
}