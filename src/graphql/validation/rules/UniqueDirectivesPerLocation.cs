using System.Collections.Generic;
using System.Linq;
using GraphQLParser.AST;

namespace fugu.graphql.validation.rules
{
    /// <summary>
    ///     Unique directive names per location
    ///     A GraphQL document is only valid if all directives at a given location
    ///     are uniquely named.
    /// </summary>
    public class UniqueDirectivesPerLocation : IValidationRule
    {
        public INodeVisitor CreateVisitor(ValidationContext context)
        {
            return new EnterLeaveListener(_ =>
            {
                _.Match<GraphQLOperationDefinition>(f => { CheckDirectives(context, f.Directives); });

                _.Match<GraphQLFieldSelection>(f => { CheckDirectives(context, f.Directives); });

                _.Match<GraphQLFragmentDefinition>(f => { CheckDirectives(context, f.Directives); });

                _.Match<GraphQLFragmentSpread>(f => { CheckDirectives(context, f.Directives); });

                _.Match<GraphQLInlineFragment>(f => { CheckDirectives(context, f.Directives); });
            });
        }

        public string DuplicateDirectiveMessage(string directiveName)
        {
            return $"The directive \"{directiveName}\" can only be used once at this location.";
        }

        private void CheckDirectives(ValidationContext context, IEnumerable<GraphQLDirective> directives)
        {
            var knownDirectives = new Dictionary<string, GraphQLDirective>();
            directives?.ToList().ForEach(directive =>
            {
                var directiveName = directive.Name.Value;
                if (knownDirectives.ContainsKey(directiveName))
                {
                    var error = new ValidationError(
                        DuplicateDirectiveMessage(directiveName),
                        knownDirectives[directiveName],
                        directive);
                    context.ReportError(error);
                }
                else
                {
                    knownDirectives[directiveName] = directive;
                }
            });
        }
    }
}