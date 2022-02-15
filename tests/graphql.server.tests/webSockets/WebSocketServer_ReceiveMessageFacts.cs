using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Tanka.GraphQL.Server.Tests.Host;
using Tanka.GraphQL.Server.WebSockets.DTOs;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Tanka.GraphQL.Server.Tests.WebSockets;

public class WebSocketServer_ReceiveMessageFacts : WebSocketFactsBase
{
    public WebSocketServer_ReceiveMessageFacts(WebApplicationFactory<Startup> factory) : base(factory)
    {
    }

    [Fact]
    public async Task ReceiveOneMessage()
    {
        /* Given */
        using var socket = await ConnectAsync();

        var message = new OperationMessage
        {
            Id = "1",
            Type = MessageType.GQL_CONNECTION_INIT
        };
        var bytes = SerializeMessage(message);

        /* When */
        await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);

        /* Then */
        var actual = await Sink.Input.ReadAsync();
        Assert.Equal(message, actual);
    }

    [Fact]
    public async Task ReceiveThreeMessages()
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
        for (var i = 0; i < messageCount; i++)
        {
            var bytes = SerializeMessage(messages[i]);
            await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
        }


        /* Then */
        for (var i = 0; i < 3; i++)
        {
            var actualMessage = await Sink.Input.ReadAsync();
            Assert.Equal(messages[i], actualMessage);
        }
    }
}