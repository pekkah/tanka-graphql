using System;
using System.IO;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.Server.Tests.Host;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Tanka.GraphQL.Server.WebSockets;
using Tanka.GraphQL.Server.WebSockets.DTOs;
using Xunit;

namespace Tanka.GraphQL.Server.Tests.WebSockets
{
    public abstract class WebSocketFactsBase : IClassFixture<WebApplicationFactory<Startup>>
    {
        private HttpClient Client;

        protected WebApplicationFactory<Startup> Factory;

        protected WebSocketFactsBase(WebApplicationFactory<Startup> factory)
        {
            Sink = new MessageSinkProtocol();
            Factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services => { services.TryAddSingleton<IProtocolHandler>(Sink); });
            });
            Client = Factory.CreateClient();
            Application = Factory.Server.Host.Services.GetRequiredService<WebSocketServer>();
        }

        public MessageSinkProtocol Sink { get; set; }

        public WebSocketServer Application { get; set; }

        protected async Task<WebSocket> ConnectAsync()
        {
            var webSocketClient = Factory.Server.CreateWebSocketClient();
            webSocketClient.ConfigureRequest = request =>
            {
                request.Headers.Add("Sec-WebSocket-Protocol", "graphql-ws");
            };
            return await webSocketClient.ConnectAsync(new Uri("http://localhost/api/graphql"), CancellationToken.None);
        }

        protected OperationMessage DeserializeMessage(string json)
        {
            return JsonConvert.DeserializeObject<OperationMessage>(json);
        }

        protected byte[] SerializeMessage(OperationMessage message)
        {
            var json = JsonConvert.SerializeObject(message);
            return Encoding.UTF8.GetBytes(json);
        }

        protected async Task<string> ReadMessage(WebSocket socket)
        {
            string message;
            var buffer = new byte[1024 * 4];
            var segment = new ArraySegment<byte>(buffer);

            using var memoryStream = new MemoryStream();
            try
            {
                WebSocketReceiveResult receiveResult;

                do
                {
                    receiveResult = await socket.ReceiveAsync(segment, CancellationToken.None);

                    if (receiveResult.CloseStatus.HasValue)
                        break;

                    if (receiveResult.Count == 0)
                        continue;

                    await memoryStream.WriteAsync(segment.Array, segment.Offset, receiveResult.Count);
                } while (!receiveResult.EndOfMessage || memoryStream.Length == 0);

                message = Encoding.UTF8.GetString(memoryStream.ToArray());

                return message;
            }
            catch (WebSocketException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}