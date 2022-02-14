using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Tanka.GraphQL.Server.Tests.Host;
using Tanka.GraphQL.Server.WebSockets.DTOs;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Tanka.GraphQL.Server.Tests.WebSockets;

public class WebSocketServer_SendMessageFacts : WebSocketFactsBase
{
    public WebSocketServer_SendMessageFacts(WebApplicationFactory<Startup> factory) : base(factory)
    {
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
        for (var i = 0; i < messageCount; i++)
        {
            var message = new OperationMessage
            {
                Id = $"{i}",
                Type = MessageType.GQL_CONNECTION_INIT
            };
            messages.Add(message);
        }

        /* When */
        for (var i = 0; i < messageCount; i++) await Application.Clients.Single().Value.Output.WriteAsync(messages[i]);


        /* Then */
        for (var i = 0; i < 3; i++)
        {
            var json = await ReadMessage(socket);
            var actualMessage = DeserializeMessage(json);
            Assert.Equal(messages[i], actualMessage);
        }
    }
}