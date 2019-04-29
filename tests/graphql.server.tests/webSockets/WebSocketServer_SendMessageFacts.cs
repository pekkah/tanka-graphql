using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using graphql.server.tests.host;
using Microsoft.AspNetCore.Mvc.Testing;
using tanka.graphql.server.webSockets.dtos;
using Xunit;

// ReSharper disable InconsistentNaming

namespace tanka.graphql.server.tests.webSockets
{
    public class WebSocketServer_SendMessageFacts : WebSocketFactsBase
    {
        public WebSocketServer_SendMessageFacts(WebApplicationFactory<Startup> factory) : base(factory)
        {
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

        [Fact]
        public async Task SendOneMessage()
        {
            /* Given */
            using var socket = await ConnectAsync();

            var message = new OperationMessage
            {
                Id = "1",
                Type = MessageType.GQL_CONNECTION_INIT
            };

            /* When */
            await Application.Clients.Single().Value.Output.WriteAsync(message);

            /* Then */

            var json = await ReadMessage(socket);
            var actual = DeserializeMessage(json);
            Assert.Equal(message, actual);
        }

        [Fact]
        public async Task SendThreeMessages()
        {
            /* Given */
            using var socket = await ConnectAsync();
            const int messageCount = 3;
            var messages = new List<OperationMessage>();
            for (int i = 0; i < messageCount; i++)
            {
                var message = new OperationMessage
                {
                    Id = $"{i}",
                    Type = MessageType.GQL_CONNECTION_INIT
                };
                messages.Add(message);
            }

            /* When */
            for (int i = 0; i < messageCount; i++)
            {
                await Application.Clients.Single().Value.Output.WriteAsync(messages[i]);
            }


            /* Then */
            for (int i = 0; i < 3; i++)
            {
                var json = await ReadMessage(socket);
                var actualMessage = DeserializeMessage(json);
                Assert.Equal(messages[i], actualMessage);
            }
        }
    }
}