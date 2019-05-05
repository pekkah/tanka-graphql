using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using graphql.server.tests.host;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using tanka.graphql.server.webSockets;
using tanka.graphql.server.webSockets.dtos;
using tanka.graphql.tests.data;
using Xunit;

namespace tanka.graphql.server.tests.webSockets
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
            await ws.SendAsync(SerializeMessage(new OperationMessage()
            {
                Id = "1",
                Type = MessageType.GQL_START,
                Payload = JObject.FromObject(new OperationMessageQueryPayload()
                {
                    Query = "{ hello }",
                    OperationName = null,
                    Variables = new Dictionary<string, object>()
                })
            }), WebSocketMessageType.Text, true, CancellationToken.None);

            /* Then */
            var json = await ReadMessage(ws);
            var message = DeserializeMessage(json);
            var executionResult = message.Payload.ToObject<ExecutionResult>();
            executionResult.ShouldMatchJson(
                @"{
                  ""data"": {
                    ""hello"": ""world""
                  }
                }");
        }
    }
}