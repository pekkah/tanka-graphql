using Microsoft.AspNetCore.Http;

namespace Tanka.GraphQL.Server
{
    public class HubServerOptions
    {
        public PathString Path { get; set; } = "/graphql";
    }
}