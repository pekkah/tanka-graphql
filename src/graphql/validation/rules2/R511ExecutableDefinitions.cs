using System.Collections.Generic;
using GraphQLParser.AST;

namespace tanka.graphql.validation.rules2
{
    /// <summary>
    ///     Formal Specification
    ///     For each definition definition in the document.
    ///     definition must be OperationDefinition or FragmentDefinition (it must not be TypeSystemDefinition).
    /// </summary>
    public class R511ExecutableDefinitions : RuleBase
    {
        public override IEnumerable<ASTNodeKind> AppliesToNodeKinds => new[] {ASTNodeKind.Document};

        public override void Visit(GraphQLDocument document, IValidationContext context)
        {
            foreach (var definition in document.Definitions)
            {
                var valid = definition.Kind == ASTNodeKind.OperationDefinition
                            || definition.Kind == ASTNodeKind.FragmentDefinition;

                if (!valid)
                    context.Error(
                        ValidationErrorCodes.R511ExecutableDefinitions,
                        "GraphQL execution will only consider the " +
                        "executable definitions Operation and Fragment. " +
                        "Type system definitions and extensions are not " +
                        "executable, and are not considered during execution.",
                        definition);
            }
        }
    }
}