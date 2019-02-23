using System.Collections.Generic;
using System.Linq;
using GraphQLParser.AST;

namespace tanka.graphql.validation.rules2
{
    /// <summary>
    ///     Formal Specification
    ///     For each operation definition operation in the document.
    ///     Let operationName be the name of operation.
    ///     If operationName exists
    ///     Let operations be all operation definitions in the document named operationName.
    ///     operations must be a set of one.
    /// </summary>
    public class R5211OperationNameUniqueness : RuleBase
    {
        public override IEnumerable<ASTNodeKind> AppliesToNodeKinds => new[]
        {
            ASTNodeKind.Document
        };

        public override void Visit(GraphQLDocument document, IValidationContext context)
        {
            if (document.Definitions.OfType<GraphQLOperationDefinition>().Count() < 2)
                return;

            var operations = document.Definitions.OfType<GraphQLOperationDefinition>()
                .ToList();

            foreach (var op in operations)
            {
                var operationName = op.Name?.Value;

                if (string.IsNullOrWhiteSpace(operationName))
                    continue;

                var matchingOperations = operations.Where(def => def.Name?.Value == operationName)
                    .ToList();

                if (matchingOperations.Count() > 1)
                {
                    context.Error(ValidationErrorCodes.R5211OperationNameUniqueness,
                        "Each named operation definition must be unique within a " +
                        "document when referred to by its name.",
                        matchingOperations);

                    break;
                }
            }
        }
    }
}