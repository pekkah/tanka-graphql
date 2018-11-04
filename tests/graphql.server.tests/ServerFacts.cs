﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using fugu.graphql.server.subscriptions;
using graphql.server.tests.host;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace fugu.graphql.server.tests
{
    public class ServerFacts : IClassFixture<WebApplicationFactory<Startup>>
    {
        public ServerFacts(WebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
            _server = factory.Server;
            _eventManager = factory.Server.Host.Services.GetRequiredService<EventManager>();
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(new Uri(_server.BaseAddress, "graphql"),
                    o => { o.HttpMessageHandlerFactory = _ => _server.CreateHandler(); })
                .Build();
        }

        private readonly HubConnection _hubConnection;
        private readonly TestServer _server;
        private HttpClient _client;
        private readonly EventManager _eventManager;

        [Fact]
        public async Task Query()
        {
            /* Given */
            var cts = new TaskCompletionSource<OperationMessage>();

            _hubConnection.On<OperationMessage>("Data", m => { cts.SetResult(m); });

            await _hubConnection.StartAsync();
            _hubConnection.Closed += exception =>
            {
                if (exception != null)
                    cts.SetException(exception);

                return Task.CompletedTask;
            };

            /* When */
            await _hubConnection.InvokeAsync("Start", new Request
            {
                Id = "1",
                Operation = new QueryOperation
                {
                    Query = "{ hello }"
                }
            });

            /* Then */
            await Task.WhenAny(cts.Task, Task.Delay(TimeSpan.FromSeconds(15)));

            var message = cts.Task.Result;
            var result = message.Payload.ToObject<ExecutionResult>();
            Assert.Equal(MessageType.GQL_DATA, message.Type);
            Assert.Contains(result.Data, kv => kv.Key == "hello" && kv.Value.ToString() == "world");

            await _hubConnection.StopAsync();
        }

        [Fact]
        public async Task Subscribe()
        {
            /* Given */
            var cts = new TaskCompletionSource<OperationMessage>();

            _hubConnection.On<OperationMessage>("Data", m => { cts.SetResult(m); });

            await _hubConnection.StartAsync();
            _hubConnection.Closed += exception =>
            {
                if (exception != null)
                    cts.SetException(exception);

                return Task.CompletedTask;
            };

            /* When */
            await _hubConnection.InvokeAsync("Start", new Request
            {
                Id = "1",
                Operation = new QueryOperation
                {
                    Query = @"
subscription { 
    helloEvents 
}"
                }
            });

            await _eventManager.Hello("world");

            /* Then */
            await Task.WhenAny(cts.Task, Task.Delay(TimeSpan.FromSeconds(15)));
            var message = cts.Task.Result;

            Assert.Equal(MessageType.GQL_DATA, message.Type);
            var result = message.Payload.ToObject<ExecutionResult>();
            Assert.Contains(result.Data, kv => kv.Key == "helloEvents" && kv.Value.ToString() == "world");

            await _hubConnection.StopAsync();
        }
    }
}