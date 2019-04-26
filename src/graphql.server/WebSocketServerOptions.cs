using Microsoft.AspNetCore.Http;

namespace tanka.graphql.server
{
    public class WebSocketServerOptions
    {
        public PathString Path { get; set; } = "/api/graphql";
    }
}