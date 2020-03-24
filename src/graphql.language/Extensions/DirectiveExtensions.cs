using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Language
{
    public static class DirectiveExtensions
    {
        public static Directive WithName(this Directive directive,
            in Name name)
        {
            return new Directive(
                name,
                directive.Arguments,
                directive.Location);
        }

        public static Directive WithArguments(this Directive directive,
            IReadOnlyCollection<Argument> arguments)
        {
            return new Directive(
                directive.Name,
                arguments,
                directive.Location);
        }
    }
}