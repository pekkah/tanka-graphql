using System.Threading;
using System.Threading.Tasks;

namespace Tanka.GraphQL.TypeSystem;

/// <summary>
/// Interface for loading GraphQL schemas from various sources
/// </summary>
public interface ISchemaLoader
{
    /// <summary>
    /// Load a GraphQL schema from the specified URL
    /// </summary>
    /// <param name="url">The URL to load the schema from (e.g., http://, file://, or relative path)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The loaded TypeSystemDocument, or null if the schema could not be loaded</returns>
    Task<Language.Nodes.TypeSystem.TypeSystemDocument?> LoadSchemaAsync(
        string url,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if this loader can handle the specified URL
    /// </summary>
    /// <param name="url">The URL to check</param>
    /// <returns>True if this loader can handle the URL, false otherwise</returns>
    bool CanLoad(string url);
}