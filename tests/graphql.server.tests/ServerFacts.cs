using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using graphql.server.tests.host;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace tanka.graphql.server.tests
{
    public class ServerFacts : IClassFixture<WebApplicationFactory<Startup>>
    {
        public ServerFacts(WebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
            _server = factory.Server;
            _eventManager = factory.Server.Host.Services.GetRequiredService<EventManager>();
        }

        private readonly TestServer _server;
        private readonly HttpClient _client;
        private readonly EventManager _eventManager;

        private HubConnection Connect()
        {
            var connection = new HubConnectionBuilder()
                .WithUrl(new Uri(_server.BaseAddress, "graphql"),
                    o => { o.HttpMessageHandlerFactory = _ => _server.CreateHandler(); })
                .Build();

            connection.Closed += exception =>
            {
                Assert.Null(exception);
                return Task.CompletedTask;
            };

            return connection;
        }

        [Fact]
        public async Task Multiple_Queries()
        {
            /* Given */
            var cts = new CancellationTokenSource();
            var hubConnection = Connect();
            await hubConnection.StartAsync();

            /* When */
            var reader1 = await hubConnection.StreamAsChannelAsync<ExecutionResult>("query", new QueryRequest
            {
                Query = "{ hello }"
            }, cts.Token);

            var reader2 = await hubConnection.StreamAsChannelAsync<ExecutionResult>("query", new QueryRequest
            {
                Query = "{ hello }"
            }, cts.Token);

            /* Then */
            var result1 = await reader1.ReadAsync();
            var result2 = await reader2.ReadAsync();

            Assert.Contains(result1.Data, kv =>
            {
                var (key, value) = kv;
                return key == "hello" && value.ToString() == "world";
            });

            Assert.Contains(result2.Data, kv =>
            {
                var (key, value) = kv;
                return key == "hello" && value.ToString() == "world";
            });

            await hubConnection.StopAsync();
        }

        [Fact]
        public async Task Mutation()
        {
            /* Given */
            var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1));
            var hubConnection = Connect();
            await hubConnection.StartAsync();

            /* When */
            var reader = await hubConnection.StreamAsChannelAsync<ExecutionResult>("Query", new QueryRequest
            {
                Query = @"
                        mutation Add($event: InputEvent!) { 
                            add(event: $event) {
                                    type
                                    message
                                }
                        }",
                Variables = new Dictionary<string, object>
                {
                    {
                        "event", new Dictionary<string, object>
                        {
                            {"type", "hello"},
                            {"message", "world"}
                        }
                    }
                }
            }, cts.Token);


            /* Then */
            var result = await reader.ReadAsync(cts.Token);

            Assert.Contains(result.Data, kv =>
            {
                var (key, value) = kv;
                return key == "add";
            });

            cts.Cancel();
            await hubConnection.StopAsync();
        }

        [Fact]
        public async Task Query()
        {
            /* Given */
            var cts = new CancellationTokenSource();
            var hubConnection = Connect();
            await hubConnection.StartAsync();

            /* When */
            var reader = await hubConnection.StreamAsChannelAsync<ExecutionResult>("query", new QueryRequest
            {
                Query = "{ hello }"
            }, cts.Token);

            /* Then */
            var result = await reader.ReadAsync(cts.Token);

            Assert.Contains(result.Data, kv =>
            {
                var (key, value) = kv;
                return key == "hello" && value.ToString() == "world";
            });

            await hubConnection.StopAsync();
        }

        [Fact]
        public async Task Subscribe()
        {
            /* Given */
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var hubConnection = Connect();
            await hubConnection.StartAsync();

            /* When */
            // this wont block until the actual hub method execution has finished?
            var reader = await hubConnection.StreamAsChannelAsync<ExecutionResult>(
                "Query",
                new QueryRequest
                {
                    Query = @"
                        subscription { 
                            events {
                                 type
                                 message
                            }
                        }"
                }, cts.Token);

            /* Then */
            var result = await reader.ReadAsync(cts.Token);

            Assert.Contains(result.Data, kv =>
            {
                var (key, value) = kv;
                return key == "events";
            });

            cts.Cancel();
            await hubConnection.StopAsync();
        }

        [Fact]
        public async Task Subscribe_with_unsubscribe()
        {
            /* Given */
            var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1));
            var hubConnection = Connect();
            await hubConnection.StartAsync();

            /* When */
            var reader = await hubConnection.StreamAsChannelAsync<ExecutionResult>("Query", new QueryRequest
            {
                Query = @"
                    subscription { 
                        events {
                             type
                             message
                        }
                    }"
            }, cts.Token);


            /* Then */
            var result = await reader.ReadAsync(cts.Token);

            Assert.Contains(result.Data, kv =>
            {
                var (key, value) = kv;
                return key == "events";
            });

            cts.Cancel();

            await hubConnection.StopAsync();
        }
    }
}