using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.TypeSystem;

/// <summary>
/// Loads GraphQL schemas from HTTP/HTTPS URLs
/// </summary>
public class HttpSchemaLoader : ISchemaLoader, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;

    /// <summary>
    /// Create a new HttpSchemaLoader with a default HttpClient
    /// </summary>
    public HttpSchemaLoader() : this(new HttpClient(), ownsHttpClient: true)
    {
    }

    /// <summary>
    /// Create a new HttpSchemaLoader with a custom HttpClient
    /// </summary>
    /// <param name="httpClient">The HttpClient to use for loading schemas</param>
    /// <param name="ownsHttpClient">Whether this loader owns the HttpClient and should dispose it</param>
    public HttpSchemaLoader(HttpClient httpClient, bool ownsHttpClient = false)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _ownsHttpClient = ownsHttpClient;
    }

    /// <inheritdoc />
    public async Task<TypeSystemDocument?> LoadSchemaAsync(string url, CancellationToken cancellationToken = default)
    {
        if (!CanLoad(url))
            return null;

        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            var parser = Parser.Create(content);
            return parser.ParseTypeSystemDocument();
        }
        catch (Exception)
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

        return url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
               url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Dispose the HttpClient if owned by this loader
    /// </summary>
    public void Dispose()
    {
        if (_ownsHttpClient)
            _httpClient?.Dispose();
    }
}