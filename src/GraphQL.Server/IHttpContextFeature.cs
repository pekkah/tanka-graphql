using Microsoft.AspNetCore.Http;

namespace Tanka.GraphQL.Server;

public interface IHttpContextFeature
{
    HttpContext HttpContext { get; set; }
}