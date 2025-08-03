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
    /// <returns>Field collection result with metadata</returns>
    FieldCollectionResult CollectFields(
        ISchema schema,
        ExecutableDocument document,
        ObjectDefinition objectDefinition,
        SelectionSet selectionSet,
        IReadOnlyDictionary<string, object?>? coercedVariableValues = null,
        List<string>? visitedFragments = null);
}

/// <summary>
/// Result of field collection including directive metadata
/// </summary>
public record FieldCollectionResult
{
    /// <summary>
    /// Grouped field selections by response key
    /// </summary>
    public required IReadOnlyDictionary<string, List<FieldSelection>> Fields { get; init; }

    /// <summary>
    /// Directive metadata for each field group, keyed by response key
    /// </summary>  
    public IReadOnlyDictionary<string, IReadOnlyDictionary<string, object>>? FieldMetadata { get; init; }
}