using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class Directive
    {
        public readonly Name Name;
        public readonly IReadOnlyCollection<Argument>? Arguments;
        public readonly Location? Location;

        public Directive(
            in Name name,
            in IReadOnlyCollection<Argument>? arguments,
            in Location? location)
        {
            Name = name;
            Arguments = arguments;
            Location = location;
        }
    }
}