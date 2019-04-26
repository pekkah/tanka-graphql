using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NSubstitute;
using tanka.graphql.server.webSockets;
using tanka.graphql.server.webSockets.dtos;
using Xunit;

// ReSharper disable InconsistentNaming

namespace tanka.graphql.server.tests.webSockets
{
    public class GraphQLWSProtocolFacts
    {
        protected ValueTask<OperationMessage> ReadWithTimeout(
            Channel<OperationMessage> channel,
            int timeoutSeconds = 10)
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));

            return channel.Reader.ReadAsync(cts.Token);
        }

        [Fact]
        public async Task Unknown()
        {
            /* Given */
            var channel = Channel.CreateUnbounded<OperationMessage>();
            var queryStreamService = Substitute.For<IQueryStreamService>();
            var sut = new GraphQLWSProtocol(queryStreamService);

            var message = new OperationMessage
            {
                Type = "ARGHH"
            };

            var context = new MessageContext(message, channel);

            /* When */
            await sut.Handle(context);

            /* Then */
            var response = await ReadWithTimeout(channel);
            Assert.Equal(new OperationMessage
            {
                Type = MessageType.GQL_CONNECTION_ERROR
            }, response);
        }

        [Fact]
        public async Task Init()
        {
            /* Given */
            var channel = Channel.CreateUnbounded<OperationMessage>();
            var queryStreamService = Substitute.For<IQueryStreamService>();
            var sut = new GraphQLWSProtocol(queryStreamService);

            var message = new OperationMessage
            {
                Type = MessageType.GQL_CONNECTION_INIT
            };

            var context = new MessageContext(message, channel);

            /* When */
            await sut.Handle(context);

            /* Then */
            var response = await ReadWithTimeout(channel);
            Assert.Equal(new OperationMessage
            {
                Type = MessageType.GQL_CONNECTION_ACK
            }, response);
        }

        [Fact]
        public async Task Terminate()
        {
            /* Given */
            var channel = Channel.CreateUnbounded<OperationMessage>();
            var queryStreamService = Substitute.For<IQueryStreamService>();
            var sut = new GraphQLWSProtocol(queryStreamService);

            var message = new OperationMessage
            {
                Type = MessageType.GQL_CONNECTION_TERMINATE
            };

            var context = new MessageContext(message, channel);

            /* When */
            await sut.Handle(context);

            /* Then */
            Assert.True(channel.Reader.Completion.IsCompleted);
        }

        [Fact]
        public async Task Start()
        {
            /* Given */
            var output = Channel.CreateUnbounded<OperationMessage>();
            var queryStreamService = Substitute.For<IQueryStreamService>();

            var queryStream = Channel.CreateUnbounded<ExecutionResult>();
            queryStreamService.QueryAsync(null, default)
                .ReturnsForAnyArgs(new QueryStream(queryStream));


            var sut = new GraphQLWSProtocol(queryStreamService);

            var message = new OperationMessage
            {
                Id = "1",
                Type = MessageType.GQL_START,
                Payload = JObject.FromObject(new OperationMessageQueryPayload())
            };

            var context = new MessageContext(message, output);

            /* When */
            await sut.Handle(context);

            /* Then */
            var subscription = sut.GetSubscription(message.Id);
            Assert.NotEqual(default, subscription);
        }

        [Fact]
        public async Task Stop()
        {
            /* Given */
            var output = Channel.CreateUnbounded<OperationMessage>();
            var queryStreamService = Substitute.For<IQueryStreamService>();

            var queryStream = Channel.CreateUnbounded<ExecutionResult>();
            queryStreamService.QueryAsync(null, default)
                .ReturnsForAnyArgs(new QueryStream(queryStream));

            var sut = new GraphQLWSProtocol(queryStreamService);
            var message = new OperationMessage
            {
                Id = "1",
                Type = MessageType.GQL_START,
                Payload = JObject.FromObject(new OperationMessageQueryPayload())
            };

            var context = new MessageContext(message, output);
            await sut.Handle(context);

            /* When */
            await sut.Handle(new MessageContext(new OperationMessage()
            {
                Id = "1",
                Type = MessageType.GQL_STOP
            }, output));

            /* Then */
            var subscription = sut.GetSubscription(message.Id);
            Assert.Equal(default, subscription);
        }
    }
}