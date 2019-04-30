using Microsoft.AspNetCore.Http;

namespace tanka.graphql.server
{
    public class HubServerOptions
    {
        public PathString Path { get; set; } = "/graphql";
    }
}