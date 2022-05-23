using Microsoft.AspNetCore.Http;

namespace Tanka.GraphQL.Server;

public class HttpContextFeature
{
    public HttpContext HttpContext { get; }

    public HttpContextFeature(HttpContext httpContext)
    {
        HttpContext = httpContext;
    }
}