using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Language
{
    public static class FieldSelectionExtensions
    {
        public static SelectionSet Merge(this IEnumerable<FieldSelection> fieldSelections)
        {
            var selections = new List<ISelection>();

            foreach (var fieldSelection in fieldSelections)
            {
                var fieldSelectionSet = fieldSelection.SelectionSet;

                if (fieldSelectionSet != null)
                    selections.AddRange(fieldSelectionSet);
            }

            return new SelectionSet(selections);
        }
    }
}