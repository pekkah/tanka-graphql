using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Tanka.GraphQL.Server.Tests.Host;
using Tanka.GraphQL.Server.WebSockets;
using Tanka.GraphQL.Server.WebSockets.DTOs;
using Tanka.GraphQL.Tests.Data;
using Xunit;

namespace Tanka.GraphQL.Server.Tests.WebSockets
{
    public class WebSocketServer_ProtocolFacts : WebSocketFactsBase
    {
        public WebSocketServer_ProtocolFacts(WebApplicationFactory<Startup> factory)
            : base(factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddScoped<IProtocolHandler, GraphQLWSProtocol>();
                });
            }))
        {
        }

        [Fact]
        public async Task Start_query()
        {
            /* Given */
            using var ws = await ConnectAsync();

            /* When */
            await ws.SendAsync(SerializeMessage(new OperationMessage
            {
                Id = "1",
                Type = MessageType.GQL_START,
                Payload = Payloads.ToQuery(new OperationMessageQueryPayload
                {
                    Query = "{ hello }",
                    OperationName = null,
                    Variables = new Dictionary<string, object>()
                })
            }), WebSocketMessageType.Text, true, CancellationToken.None);

            /* Then */
            var json = await ReadMessage(ws);
            var message = DeserializeMessage(json);
            var executionResult = Payloads.GetResult(message.Payload);
            executionResult.ShouldMatchJson(
                @"{
                  ""data"": {
                    ""hello"": ""world""
                  }
                }");
        }
    }
}