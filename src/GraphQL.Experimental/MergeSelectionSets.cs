using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Experimental
{
    public delegate SelectionSet MergeSelectionSets(IReadOnlyList<FieldSelection> fields);
}