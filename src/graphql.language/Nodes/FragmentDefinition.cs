using System;
using System.Collections.Generic;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class FragmentDefinition
    {
        public readonly IReadOnlyCollection<Directive>? Directives;
        public readonly Name FragmentName;
        public readonly Location? Location;
        public readonly SelectionSet SelectionSet;
        public readonly NamedType TypeCondition;

        public FragmentDefinition(
            in Name fragmentName,
            NamedType typeCondition,
            IReadOnlyCollection<Directive>? directives,
            SelectionSet selectionSet,
            in Location? location = default)
        {
            FragmentName = fragmentName;
            TypeCondition = typeCondition;
            Directives = directives;
            SelectionSet = selectionSet;
            Location = location;
        }

        public static implicit operator FragmentDefinition(string value)
        {
            var parser = new Parser(Encoding.UTF8.GetBytes(value));
            return parser.ParseFragmentDefinition();
        }

        public static implicit operator string(FragmentDefinition value)
        {
            throw new NotImplementedException();
        }
    }
}