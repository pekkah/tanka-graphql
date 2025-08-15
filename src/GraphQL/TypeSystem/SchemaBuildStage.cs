namespace Tanka.GraphQL.TypeSystem;

/// <summary>
/// Stages in the schema build pipeline
/// </summary>
public enum SchemaBuildStage
{
    /// <summary>
    /// Initial setup and configuration application
    /// </summary>
    Initialization = 0,

    /// <summary>
    /// Type collection and organization
    /// </summary>
    TypeCollection = 1,

    /// <summary>
    /// Processing @link directives and importing schemas
    /// </summary>
    LinkProcessing = 2,

    /// <summary>
    /// Processing after link imports are complete (Federation uses this)
    /// </summary>
    PostLinkProcessing = 3,

    /// <summary>
    /// Type resolution and reference resolution
    /// </summary>
    TypeResolution = 4,

    /// <summary>
    /// Schema validation
    /// </summary>
    Validation = 5,

    /// <summary>
    /// Final schema construction
    /// </summary>
    Finalization = 6
}