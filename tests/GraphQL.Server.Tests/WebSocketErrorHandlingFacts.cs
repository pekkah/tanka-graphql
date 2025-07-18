using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tanka.GraphQL.Server.WebSockets;
using Xunit;

namespace Tanka.GraphQL.Server.Tests;

public class WebSocketErrorHandlingFacts
{
    [Fact]
    public async Task WebSocketChannel_ShouldHandleConnectionAborted()
    {
        // Given
        var webSocket = Substitute.For<WebSocket>();
        var logger = Substitute.For<ILogger<WebSocketChannel>>();
        var cancellationToken = new CancellationTokenSource().Token;
        
        webSocket.State.Returns(WebSocketState.Aborted);
        
        var channel = new WebSocketChannel(webSocket, logger);
        
        // When & Then
        await Assert.ThrowsAsync<WebSocketException>(() => 
            channel.SendAsync(new ConnectionInit(), cancellationToken));
    }

    [Fact]
    public async Task WebSocketChannel_ShouldHandleConnectionClosed()
    {
        // Given
        var webSocket = Substitute.For<WebSocket>();
        var logger = Substitute.For<ILogger<WebSocketChannel>>();
        var cancellationToken = new CancellationTokenSource().Token;
        
        webSocket.State.Returns(WebSocketState.Closed);
        
        var channel = new WebSocketChannel(webSocket, logger);
        
        // When & Then
        await Assert.ThrowsAsync<WebSocketException>(() => 
            channel.SendAsync(new ConnectionInit(), cancellationToken));
    }

    [Fact]
    public async Task WebSocketChannel_ShouldHandleInvalidMessage()
    {
        // Given
        var webSocket = Substitute.For<WebSocket>();
        var logger = Substitute.For<ILogger<WebSocketChannel>>();
        var cancellationToken = new CancellationTokenSource().Token;
        
        webSocket.State.Returns(WebSocketState.Open);
        
        var invalidJson = "invalid json";
        var buffer = Encoding.UTF8.GetBytes(invalidJson);
        
        webSocket.ReceiveAsync(Arg.Any<ArraySegment<byte>>(), cancellationToken)
            .Returns(Task.FromResult(new WebSocketReceiveResult(
                buffer.Length, 
                WebSocketMessageType.Text, 
                true)));
        
        var channel = new WebSocketChannel(webSocket, logger);
        
        // When & Then
        await Assert.ThrowsAsync<JsonException>(() => 
            channel.ReceiveAsync(cancellationToken));
    }

    [Fact]
    public async Task WebSocketChannel_ShouldHandleMessageTooLarge()
    {
        // Given
        var webSocket = Substitute.For<WebSocket>();
        var logger = Substitute.For<ILogger<WebSocketChannel>>();
        var cancellationToken = new CancellationTokenSource().Token;
        
        webSocket.State.Returns(WebSocketState.Open);
        
        // Simulate a message that's too large
        var largeMessage = new string('a', 1024 * 1024); // 1MB
        var buffer = Encoding.UTF8.GetBytes(largeMessage);
        
        webSocket.ReceiveAsync(Arg.Any<ArraySegment<byte>>(), cancellationToken)
            .Returns(Task.FromResult(new WebSocketReceiveResult(
                buffer.Length, 
                WebSocketMessageType.Text, 
                false))); // Not end of message
        
        var channel = new WebSocketChannel(webSocket, logger);
        
        // When & Then
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            channel.ReceiveAsync(cancellationToken));
    }

    [Fact]
    public async Task WebSocketChannel_ShouldHandleCancellation()
    {
        // Given
        var webSocket = Substitute.For<WebSocket>();
        var logger = Substitute.For<ILogger<WebSocketChannel>>();
        var cancellationTokenSource = new CancellationTokenSource();
        
        webSocket.State.Returns(WebSocketState.Open);
        
        webSocket.ReceiveAsync(Arg.Any<ArraySegment<byte>>(), cancellationTokenSource.Token)
            .Returns(Task.FromCanceled<WebSocketReceiveResult>(cancellationTokenSource.Token));
        
        var channel = new WebSocketChannel(webSocket, logger);
        
        // When
        cancellationTokenSource.Cancel();
        
        // Then
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            channel.ReceiveAsync(cancellationTokenSource.Token));
    }

    [Fact]
    public async Task WebSocketChannel_ShouldHandleUnexpectedMessageType()
    {
        // Given
        var webSocket = Substitute.For<WebSocket>();
        var logger = Substitute.For<ILogger<WebSocketChannel>>();
        var cancellationToken = new CancellationTokenSource().Token;
        
        webSocket.State.Returns(WebSocketState.Open);
        
        var binaryData = new byte[] { 0x01, 0x02, 0x03 };
        
        webSocket.ReceiveAsync(Arg.Any<ArraySegment<byte>>(), cancellationToken)
            .Returns(Task.FromResult(new WebSocketReceiveResult(
                binaryData.Length, 
                WebSocketMessageType.Binary, // Unexpected binary message
                true)));
        
        var channel = new WebSocketChannel(webSocket, logger);
        
        // When & Then
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            channel.ReceiveAsync(cancellationToken));
    }

    [Fact]
    public async Task WebSocketChannel_ShouldHandleCloseMessage()
    {
        // Given
        var webSocket = Substitute.For<WebSocket>();
        var logger = Substitute.For<ILogger<WebSocketChannel>>();
        var cancellationToken = new CancellationTokenSource().Token;
        
        webSocket.State.Returns(WebSocketState.Open);
        
        webSocket.ReceiveAsync(Arg.Any<ArraySegment<byte>>(), cancellationToken)
            .Returns(Task.FromResult(new WebSocketReceiveResult(
                0, 
                WebSocketMessageType.Close, 
                true)));
        
        var channel = new WebSocketChannel(webSocket, logger);
        
        // When
        var result = await channel.ReceiveAsync(cancellationToken);
        
        // Then
        Assert.Null(result);
    }

    [Fact]
    public async Task WebSocketChannel_ShouldHandleFragmentedMessage()
    {
        // Given
        var webSocket = Substitute.For<WebSocket>();
        var logger = Substitute.For<ILogger<WebSocketChannel>>();
        var cancellationToken = new CancellationTokenSource().Token;
        
        webSocket.State.Returns(WebSocketState.Open);
        
        var message = JsonSerializer.Serialize(new ConnectionInit());
        var messageBytes = Encoding.UTF8.GetBytes(message);
        
        var fragment1 = messageBytes[0..(messageBytes.Length / 2)];
        var fragment2 = messageBytes[(messageBytes.Length / 2)..];
        
        webSocket.ReceiveAsync(Arg.Any<ArraySegment<byte>>(), cancellationToken)
            .Returns(
                Task.FromResult(new WebSocketReceiveResult(
                    fragment1.Length, 
                    WebSocketMessageType.Text, 
                    false)), // Not end of message
                Task.FromResult(new WebSocketReceiveResult(
                    fragment2.Length, 
                    WebSocketMessageType.Text, 
                    true))); // End of message
        
        var channel = new WebSocketChannel(webSocket, logger);
        
        // When
        var result = await channel.ReceiveAsync(cancellationToken);
        
        // Then
        Assert.IsType<ConnectionInit>(result);
    }

    [Fact]
    public async Task WebSocketChannel_ShouldHandleEmptyMessage()
    {
        // Given
        var webSocket = Substitute.For<WebSocket>();
        var logger = Substitute.For<ILogger<WebSocketChannel>>();
        var cancellationToken = new CancellationTokenSource().Token;
        
        webSocket.State.Returns(WebSocketState.Open);
        
        webSocket.ReceiveAsync(Arg.Any<ArraySegment<byte>>(), cancellationToken)
            .Returns(Task.FromResult(new WebSocketReceiveResult(
                0, 
                WebSocketMessageType.Text, 
                true)));
        
        var channel = new WebSocketChannel(webSocket, logger);
        
        // When & Then
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            channel.ReceiveAsync(cancellationToken));
    }

    [Fact]
    public async Task SubscriptionManager_ShouldHandleSubscriptionFailure()
    {
        // Given
        var subscriptionManager = new SubscriptionManager();
        var subscriptionId = "test-subscription";
        var cancellationToken = new CancellationTokenSource().Token;
        
        // When
        subscriptionManager.AddSubscription(subscriptionId, CreateFailingAsyncEnumerable());
        
        // Then
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            subscriptionManager.GetSubscriptionAsync(subscriptionId, cancellationToken));
    }

    [Fact]
    public async Task SubscriptionManager_ShouldHandleSubscriptionCancellation()
    {
        // Given
        var subscriptionManager = new SubscriptionManager();
        var subscriptionId = "test-subscription";
        var cancellationTokenSource = new CancellationTokenSource();
        
        // When
        subscriptionManager.AddSubscription(subscriptionId, CreateLongRunningAsyncEnumerable());
        cancellationTokenSource.Cancel();
        
        // Then
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            subscriptionManager.GetSubscriptionAsync(subscriptionId, cancellationTokenSource.Token));
    }

    [Fact]
    public void SubscriptionManager_ShouldHandleNonExistentSubscription()
    {
        // Given
        var subscriptionManager = new SubscriptionManager();
        var subscriptionId = "non-existent";
        
        // When & Then
        Assert.False(subscriptionManager.HasSubscription(subscriptionId));
    }

    [Fact]
    public void SubscriptionManager_ShouldHandleDoubleRemoval()
    {
        // Given
        var subscriptionManager = new SubscriptionManager();
        var subscriptionId = "test-subscription";
        
        subscriptionManager.AddSubscription(subscriptionId, CreateSimpleAsyncEnumerable());
        
        // When
        subscriptionManager.RemoveSubscription(subscriptionId);
        subscriptionManager.RemoveSubscription(subscriptionId); // Second removal
        
        // Then
        Assert.False(subscriptionManager.HasSubscription(subscriptionId));
    }

    private static async IAsyncEnumerable<ExecutionResult> CreateFailingAsyncEnumerable()
    {
        await Task.Delay(10);
        throw new InvalidOperationException("Subscription failed");
        yield break;
    }

    private static async IAsyncEnumerable<ExecutionResult> CreateLongRunningAsyncEnumerable()
    {
        for (int i = 0; i < 1000; i++)
        {
            await Task.Delay(100);
            yield return new ExecutionResult { Data = new { Value = i } };
        }
    }

    private static async IAsyncEnumerable<ExecutionResult> CreateSimpleAsyncEnumerable()
    {
        await Task.Delay(10);
        yield return new ExecutionResult { Data = new { Value = "test" } };
    }
}