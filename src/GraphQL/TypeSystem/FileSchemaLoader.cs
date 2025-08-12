using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.TypeSystem;

/// <summary>
/// Loads GraphQL schemas from the file system
/// </summary>
public class FileSchemaLoader : ISchemaLoader
{
    private readonly string? _basePath;

    /// <summary>
    /// Create a new FileSchemaLoader
    /// </summary>
    /// <param name="basePath">Optional base path for resolving relative URLs</param>
    public FileSchemaLoader(string? basePath = null)
    {
        _basePath = basePath;
    }

    /// <inheritdoc />
    public async Task<TypeSystemDocument?> LoadSchemaAsync(string url, CancellationToken cancellationToken = default)
    {
        if (!CanLoad(url))
            return null;

        try
        {
            var filePath = GetFilePath(url);

            if (!File.Exists(filePath))
                return null;

            var content = await File.ReadAllTextAsync(filePath, cancellationToken);

            var parser = Parser.Create(content);
            return parser.ParseTypeSystemDocument();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Security.SecurityException)
        {
            // Log error if logging is available
            return null;
        }
    }

    /// <inheritdoc />
    public bool CanLoad(string url)
    {
        if (string.IsNullOrEmpty(url))
            return false;

        // Handle file:// URLs
        if (url.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
            return true;

        // Handle relative paths (don't start with a protocol)
        if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            return true;

        return false;
    }

    private string GetFilePath(string url)
    {
        // Handle file:// URLs
        if (url.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
        {
            var uri = new Uri(url);
            return uri.LocalPath;
        }

        // Handle relative paths
        if (!Path.IsPathRooted(url) && !string.IsNullOrEmpty(_basePath))
        {
            return Path.GetFullPath(Path.Combine(_basePath, url));
        }

        return Path.GetFullPath(url);
    }
}