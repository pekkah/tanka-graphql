using Microsoft.AspNetCore.Http;

namespace Tanka.GraphQL.Server;

public class HttpContextFeature : IHttpContextFeature
{
    public HttpContext HttpContext { get; set; } = default!;
}