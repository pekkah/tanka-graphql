using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.SelectionSets;

public static class SelectionSetExtensions
{
    public static SelectionSet MergeSelectionSets(this IReadOnlyCollection<FieldSelection> fields)
    {
        var selectionSet = new List<ISelection>();
        foreach (FieldSelection field in fields)
        {
            SelectionSet? fieldSelectionSet = field.SelectionSet;
            if (fieldSelectionSet is null || fieldSelectionSet.Count == 0) continue;

            selectionSet.AddRange(fieldSelectionSet);
        }

        return new SelectionSet(selectionSet);
    }
}