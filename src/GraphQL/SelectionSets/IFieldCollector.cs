using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.SelectionSets;

/// <summary>
/// Service for collecting fields from selection sets, handling directive processing
/// </summary>
public interface IFieldCollector
{
    /// <summary>
    /// Collect fields from a selection set, processing directives and fragments
    /// </summary>
    /// <param name="schema">GraphQL schema</param>
    /// <param name="document">Executable document containing the query</param>
    /// <param name="objectDefinition">Object type being selected from</param>
    /// <param name="selectionSet">Selection set to process</param>
    /// <param name="coercedVariableValues">Variable values for directive evaluation</param>
    /// <param name="visitedFragments">Fragment names already visited (for recursion detection)</param>
    /// <returns>Grouped field selections by response key</returns>
    IReadOnlyDictionary<string, List<FieldSelection>> CollectFields(
        ISchema schema,
        ExecutableDocument document,
        ObjectDefinition objectDefinition,
        SelectionSet selectionSet,
        IReadOnlyDictionary<string, object?>? coercedVariableValues = null,
        List<string>? visitedFragments = null);
}