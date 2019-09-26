using Microsoft.AspNetCore.Http;

namespace Tanka.GraphQL.Server
{
    public class WebSocketServerOptions
    {
        public PathString Path { get; set; } = "/api/graphql";
    }
}