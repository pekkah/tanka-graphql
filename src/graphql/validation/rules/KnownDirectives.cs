using System;
using System.Linq;
using fugu.graphql.type;
using GraphQLParser.AST;

namespace fugu.graphql.validation.rules
{
    /// <summary>
    ///     Known directives
    ///     A GraphQL document is only valid if all `@directives` are known by the
    ///     schema and legally positioned.
    /// </summary>
    public class KnownDirectives : IValidationRule
    {
        public INodeVisitor CreateVisitor(ValidationContext context)
        {
            return new EnterLeaveListener(_ =>
            {
                _.Match<GraphQLDirective>(node =>
                {
                    var name = node.Name.Value;
                    var directiveDef = context.Schema.GetDirective(name);
                    if (directiveDef == null)
                    {
                        context.ReportError(new ValidationError(
                            UnknownDirectiveMessage(name), node));
                        return;
                    }

                    var candidateLocation = GetDirectiveLocationForAstPath(context.TypeInfo.GetAncestors(), context);
                    if (directiveDef.Locations.All(x => x != candidateLocation))
                        context.ReportError(new ValidationError(
                            MisplacedDirectiveMessage(name, candidateLocation.ToString()),
                            node));
                });
            });
        }

        public string UnknownDirectiveMessage(string directiveName)
        {
            return $"Unknown directive \"{directiveName}\".";
        }

        public string MisplacedDirectiveMessage(string directiveName, string location)
        {
            return $"Directive \"{directiveName}\" may not be used on {location}.";
        }


        private DirectiveLocation GetDirectiveLocationForAstPath(ASTNode[] ancestors, ValidationContext context)
        {
            var appliedTo = ancestors[ancestors.Length - 1];
            /*
            if (appliedTo is Directives || appliedTo is GraphQLArguments)
            {
                appliedTo = ancestors[ancestors.Length - 2];
            }*/

            switch (appliedTo)
            {
                case GraphQLOperationDefinition op:
                    switch (op.Operation)
                    {
                        case OperationType.Query: return DirectiveLocation.QUERY;
                        case OperationType.Mutation: return DirectiveLocation.MUTATION;
                        case OperationType.Subscription: return DirectiveLocation.SUBSCRIPTION;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                case GraphQLFieldSelection _:
                    return DirectiveLocation.FIELD;
                case GraphQLFragmentSpread _:
                    return DirectiveLocation.FRAGMENT_SPREAD;
                case GraphQLFragmentDefinition _:
                    return DirectiveLocation.FRAGMENT_DEFINITION;
                case GraphQLInlineFragment _:
                    return DirectiveLocation.INLINE_FRAGMENT;
                default:
                    throw new ArgumentOutOfRangeException(nameof(appliedTo));
            }
        }
    }
}