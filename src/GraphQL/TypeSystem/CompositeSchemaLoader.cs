using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.TypeSystem;

/// <summary>
/// Chains multiple schema loaders to support loading from various sources
/// </summary>
public class CompositeSchemaLoader : ISchemaLoader
{
    private readonly IReadOnlyList<ISchemaLoader> _loaders;

    /// <summary>
    /// Create a new CompositeSchemaLoader
    /// </summary>
    /// <param name="loaders">The loaders to chain, in order of precedence</param>
    public CompositeSchemaLoader(params ISchemaLoader[] loaders)
    {
        if (loaders == null || loaders.Length == 0)
            throw new ArgumentException("At least one loader must be provided", nameof(loaders));

        _loaders = loaders.ToList();
    }

    /// <summary>
    /// Create a new CompositeSchemaLoader
    /// </summary>
    /// <param name="loaders">The loaders to chain, in order of precedence</param>
    public CompositeSchemaLoader(IEnumerable<ISchemaLoader> loaders)
    {
        _loaders = loaders?.ToList() ?? throw new ArgumentNullException(nameof(loaders));

        if (_loaders.Count == 0)
            throw new ArgumentException("At least one loader must be provided", nameof(loaders));
    }

    /// <inheritdoc />
    public async Task<TypeSystemDocument?> LoadSchemaAsync(string url, CancellationToken cancellationToken = default)
    {
        foreach (var loader in _loaders)
        {
            if (loader.CanLoad(url))
            {
                var result = await loader.LoadSchemaAsync(url, cancellationToken);
                if (result != null)
                    return result;
            }
        }

        return null;
    }

    /// <inheritdoc />
    public bool CanLoad(string url)
    {
        return _loaders.Any(loader => loader.CanLoad(url));
    }

    /// <summary>
    /// Create a default composite loader with HTTP and File support
    /// </summary>
    /// <param name="basePath">Optional base path for file resolution</param>
    /// <returns>A composite loader supporting HTTP and file URLs</returns>
    public static CompositeSchemaLoader CreateDefault(string? basePath = null)
    {
        return new CompositeSchemaLoader(
            new HttpSchemaLoader(),
            new FileSchemaLoader(basePath)
        );
    }
}