using tanka.graphql.type;
using GraphQLParser.AST;

namespace tanka.graphql.validation.rules
{
    /// <summary>
    ///     Possible fragment spread
    ///     A fragment spread is only valid if the type condition could ever possibly
    ///     be true: if there is a non-empty intersection of the possible parent types,
    ///     and possible types which pass the type condition.
    /// </summary>
    public class PossibleFragmentSpreads : IValidationRule
    {
        public INodeVisitor CreateVisitor(ValidationContext context)
        {
            return new EnterLeaveListener(_ =>
            {
                _.Match<GraphQLInlineFragment>(node =>
                {
                    var fragType = context.TypeInfo.GetLastType();
                    var parentType = context.TypeInfo.GetParentType().Unwrap();

                    /*if (fragType != null && parentType != null && !context.Schema.DoTypesOverlap(fragType, parentType))
                        context.ReportError(new ValidationError(
                            TypeIncompatibleAnonSpreadMessage(context.Print(parentType), context.Print(fragType)),
                            node));*/
                });

                _.Match<GraphQLFragmentSpread>(node =>
                {
                    var fragName = node.Name.Value;
                    var fragType = GetFragmentType(context, fragName);
                    var parentType = context.TypeInfo.GetParentType().Unwrap();

                    /*if (fragType != null && parentType != null && !context.Schema.DoTypesOverlap(fragType, parentType))
                        context.ReportError(new ValidationError(
                            TypeIncompatibleSpreadMessage(fragName, context.Print(parentType), context.Print(fragType)),
                            node));*/
                });
            });
        }

        public string TypeIncompatibleSpreadMessage(string fragName, string parentType, string fragType)
        {
            return
                $"Fragment \"{fragName}\" cannot be spread here as objects of type \"{parentType}\" can never be of type \"{fragType}\".";
        }

        public string TypeIncompatibleAnonSpreadMessage(string parentType, string fragType)
        {
            return
                $"Fragment cannot be spread here as objects of type \"{parentType}\" can never be of type \"{fragType}\".";
        }

        private static IType GetFragmentType(ValidationContext context, string name)
        {
            var frag = context.GetFragment(name);
            if (frag == null)
                return null;

            return Ast.TypeFromAst(context.Schema, frag.TypeCondition);
        }
    }
}