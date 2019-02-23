using System.Collections.Generic;
using System.Linq;
using GraphQLParser.AST;

namespace tanka.graphql.validation.rules2
{
    /// <summary>
    ///     For each fragment definition fragment in the document
    ///     Let fragmentName be the name of fragment.
    ///     Let fragments be all fragment definitions in the document named fragmentName.
    ///     fragments must be a set of one.
    /// </summary>
    public class R5511FragmentNameUniqueness : RuleBase
    {
        public override IEnumerable<ASTNodeKind> AppliesToNodeKinds =>
            new[]
            {
                ASTNodeKind.FragmentDefinition
            };

        public override void BeginVisitFragmentDefinition(GraphQLFragmentDefinition node, IValidationContext context)
        {
            if (context.Fragments.Any(f => f.Name.Value == node.Name.Value))
            {
                context.Error(
                    ValidationErrorCodes.R5511FragmentNameUniqueness,
                    "Fragment definitions are referenced in fragment spreads by name. To avoid " +
                    "ambiguity, each fragment’s name must be unique within a document.",
                    node);

            }
        }
    }
}