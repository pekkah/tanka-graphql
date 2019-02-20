using System.Collections.Generic;
using GraphQLParser.AST;

namespace tanka.graphql.validation.rules2
{
    public class R511ExecutableDefinitions : Rule
    {
        public override IEnumerable<ASTNodeKind> AppliesToNodeKinds => new[] {ASTNodeKind.Document};

        public override IEnumerable<ValidationError> Visit(GraphQLDocument document)
        {
            foreach (var definition in document.Definitions)
            {
                var valid = definition.Kind == ASTNodeKind.OperationDefinition
                            || definition.Kind == ASTNodeKind.FragmentDefinition;

                if (!valid)
                    yield return new ValidationError(
                        Errors.R511ExecutableDefinitions,
                        "GraphQL execution will only consider the " +
                        "executable definitions Operation and Fragment. " +
                        "Type system definitions and extensions are not " +
                        "executable, and are not considered during execution.",
                        definition);
            }
        }
    }
}