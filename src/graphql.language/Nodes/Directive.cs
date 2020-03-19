using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class Directive
    {
        public readonly IReadOnlyCollection<Argument>? Arguments;
        public readonly Location? Location;
        public readonly Name Name;

        public Directive(
            Name name,
            IReadOnlyCollection<Argument>? arguments,
            in Location? location)
        {
            Name = name;
            Arguments = arguments;
            Location = location;
        }
    }
}