using System;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using Tanka.GraphQL.Mock.Data;
using Tanka.GraphQL.Server.WebSockets;

using Xunit;

namespace Tanka.GraphQL.Server.Tests;

public class ServerFacts: IAsyncDisposable
{
    private readonly TankaGraphQLServerFactory _factory = new();

    [Fact]
    public async Task Connect_and_close()
    {
        /* Given */
        var webSocket = await Connect();

        /* When */
        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Test complete", CancellationToken.None);
    }

    [Fact]
    public async Task Connect_and_abort()
    {
        /* Given */
        var webSocket = await Connect();

        /* When */
        /* Then */
        webSocket.Abort();
    }

    [Fact]
    public async Task Message_Init()
    {
        /* Given */
        var webSocket = await Connect();
        var cancelReceive = new CancellationTokenSource(TimeSpan.FromSeconds(360));
        
        /* When */
        await webSocket.Send(new ConnectionInit());
        var ack = await webSocket.Receive(cancelReceive.Token);
        
        /* Then */
        Assert.IsType<ConnectionAck>(ack);

        /* Finally */
        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Test complete", CancellationToken.None);
    }

    [Fact]
    public async Task Message_Subscribe_with_query()
    {
        /* Given */
        var webSocket = await Connect(true);

        /* When */
        await webSocket.Send(new Subscribe()
        {
            Id = Guid.NewGuid().ToString(),
            Payload = new GraphQLHttpRequest()
            {
                Query = "query { hello }"
            }
        });

        /* Then */
        var message = await webSocket.Receive();
        var next = Assert.IsType<Next>(message);
        next.Payload.ShouldMatchJson(
            """
            {
              "data": {
                "hello": "Hello World!"
              }
            }                     
            """);
        

        /* Finally */
        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Test complete", CancellationToken.None);
    }

    [Fact]
    public async Task Message_Subscribe_with_subscription()
    {
        /* Given */
        var webSocket = await Connect(true);

        /* When */
        await webSocket.Send(new Subscribe()
        {
            Id = Guid.NewGuid().ToString(),
            Payload = new GraphQLHttpRequest()
            {
                Query = """
                        subscription 
                        { 
                            events 
                            { 
                                id 
                            } 
                        }
                        """
            }
        });

        await _factory.Events.WaitForSubscribers(TimeSpan.FromSeconds(30));
        /* Then */
        var eventId = Guid.NewGuid().ToString();
        await _factory.Events.Publish(new MessageEvent()
        {
            Id = eventId
        });
        var message = await webSocket.Receive();
        var next = Assert.IsType<Next>(message);
        next.Payload.ShouldMatchJson(
            $$"""
            {
              "data": {
                "events": {
                  "id": "{{eventId}}"
                }
              }
            }
            """);


        /* Finally */
        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Test complete", CancellationToken.None);
    }

    [Fact]
    public async Task Multiple_subscriptions()
    {
        /* Given */
        var webSocket = await Connect(true);

        /* When */
        var id1 = "1";
        await webSocket.Send(new Subscribe()
        {
            Id = id1,
            Payload = new GraphQLHttpRequest()
            {
                Query = """
                        subscription
                        {
                            events
                            {
                                id
                            }
                        }
                        """
            }
        });

        var id2 = "2";
        await webSocket.Send(new Subscribe()
        {
            Id = id2,
            Payload = new GraphQLHttpRequest()
            {
                Query = """
                        subscription
                        {
                            events
                            {
                                id
                            }
                        }
                        """
            }
        });

        await _factory.Events.WaitForAtLeastSubscribers(TimeSpan.FromSeconds(30), 2);
        
        /* Then */
        var eventId = Guid.NewGuid().ToString();
        await _factory.Events.Publish(new MessageEvent()
        {
            Id = eventId
        });
        var message = await webSocket.Receive();
        var next = Assert.IsType<Next>(message);
        next.Payload.ShouldMatchJson(
            $$"""
              {
                "data": {
                  "events": {
                    "id": "{{eventId}}"
                  }
                }
              }
              """);


        /* Finally */
        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Test complete", CancellationToken.None);
    }

    [Fact]
    public async Task Message_Complete()
    {
        /* Given */
        var id = Guid.NewGuid().ToString();
        var webSocket = await Connect(true);
        await webSocket.Send(new Subscribe()
        {
            Id = id,
            Payload = new GraphQLHttpRequest()
            {
                Query = """
                        subscription
                        {
                            events
                            {
                                id
                            }
                        }
                        """
            }
        });

        await _factory.Events.WaitForSubscribers(TimeSpan.FromSeconds(30));
       
        /* When */
        await webSocket.Send(new Complete()
        {
            Id = id
        });
        
        await _factory.Events.WaitForNoSubscribers(TimeSpan.FromSeconds(30));
        
        /* Then */
        Assert.Equal(0, _factory.Events.SubscriberCount);

        /* Finally */
        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Test complete", CancellationToken.None);
    }
    
    [Fact]
    public async Task DirectEchoMultipleFacts()
    {
        /* Given */
        var webSocket = await Connect(false, EchoProtocol.Protocol);


        /* When */
        await webSocket.Send(new Subscribe()
        {
            Id = "1",
            Payload = new GraphQLHttpRequest()
            {
                Query = "query { hello }"
            }
        });

        await webSocket.Send(new Subscribe()
        {
            Id = "2",
            Payload = new GraphQLHttpRequest()
            {
                Query = "query { hello }"
            }
        });


        var message1 = await webSocket.Receive(TimeSpan.FromSeconds(360));
        var message2 = await webSocket.Receive(TimeSpan.FromSeconds(360));

        /* Then */
        Assert.IsType<Subscribe>(message1);
        Assert.IsType<Subscribe>(message2);
    }

    [Fact]
    public async Task DirectEchoMultipleAlternativeFacts()
    {
        /* Given */
        var webSocket = await Connect(false, EchoProtocol.Protocol);


        /* When */
        await webSocket.Send(new Subscribe()
        {
            Id = "1",
            Payload = new GraphQLHttpRequest()
            {
                Query = "query { hello }"
            }
        });
        var message1 = await webSocket.Receive(TimeSpan.FromSeconds(360));
        
        await webSocket.Send(new Subscribe()
        {
            Id = "2",
            Payload = new GraphQLHttpRequest()
            {
                Query = "query { hello }"
            }
        });
        var message2 = await webSocket.Receive(TimeSpan.FromSeconds(360));

        /* Then */
        Assert.IsType<Subscribe>(message1);
        Assert.IsType<Subscribe>(message2);
    }

    [Fact]
    public async Task DirectEchoFacts()
    {
        /* Given */
        var webSocket = await Connect(false, EchoProtocol.Protocol);


        /* When */
        await webSocket.Send(new Subscribe()
        {
            Id = "1",
            Payload = new GraphQLHttpRequest()
            {
                Query = "query { hello }"
            }
        });
        
        var message1 = await webSocket.Receive(TimeSpan.FromSeconds(360));

        /* Then */
        Assert.IsType<Subscribe>(message1);
    }

    private async Task<WebSocket> Connect(bool connectionInit = false, string protocol = "graphql-transport-ws")
    {
        var client = _factory.CreateWebSocketClient();
        client.SubProtocols.Add(protocol);
        var webSocket = await client.ConnectAsync(new Uri("ws://localhost/graphql/ws"), CancellationToken.None);

        if (connectionInit)
        {
            await webSocket.Send(new ConnectionInit());
            using var cancelReceive = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var ack = await webSocket.Receive(cancelReceive.Token);
            Assert.IsType<ConnectionAck>(ack);
        }

        return webSocket;
    }

    public async ValueTask DisposeAsync()
    {
        if (_factory != null) await _factory.DisposeAsync();
    }
}