using System.Threading.Tasks;

namespace Tanka.GraphQL.TypeSystem;

/// <summary>
/// Delegate for schema build pipeline
/// </summary>
public delegate Task<ISchema> SchemaBuildDelegate(ISchemaBuildContext context);

/// <summary>
/// Interface for schema build middleware
/// </summary>
public interface ISchemaBuildMiddleware
{
    /// <summary>
    /// Invoke the middleware
    /// </summary>
    Task<ISchema> InvokeAsync(ISchemaBuildContext context, SchemaBuildDelegate next);
}