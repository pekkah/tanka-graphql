using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using fugu.graphql.server.subscriptions;
using fugu.graphql.server.tests.subscriptions.specs;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Xunit;

namespace fugu.graphql.server.tests.subscriptions
{
    public class ProtocolHandlerFacts
    {
        private TestableSubscriptionTransport _transport;
        private SubscriptionManager _subscriptionManager;
        private SubscriptionServer _server;
        private ApolloProtocol _sut;
        private IExecutor _executor;

        public ProtocolHandlerFacts()
        {
            _transport = new TestableSubscriptionTransport();
            _executor = Substitute.For<IExecutor>();
            _subscriptionManager = new SubscriptionManager(_executor, new NullLoggerFactory());
            _sut = new ApolloProtocol(new NullLogger<ApolloProtocol>());
            _server = new SubscriptionServer(
                _transport,
                _subscriptionManager,
                new[] { _sut },
                new NullLogger<SubscriptionServer>());
        }

        [Fact]
        public async Task Receive_init()
        {
            /* Given */
            var expected = new OperationMessage
            {
                Type = MessageType.GQL_CONNECTION_INIT
            };
            _transport.AddMessageToRead(expected);
            _transport.Complete();

            /* When */
            await _server.OnConnect();
            await _transport.Completion;

            /* Then */
            Assert.Contains(_transport.WrittenMessages,
                message => message.Type == MessageType.GQL_CONNECTION_ACK);
        }

        [Fact]
        public async Task Receive_start_mutation()
        {
            /* Given */
            var expected = new OperationMessage
            {
                Type = MessageType.GQL_START,
                Id = "1",
                Payload = JObject.FromObject(new OperationMessagePayload
                {
                    Query = @"mutation AddMessage($message: MessageInputType!) {
  addMessage(message: $message) {
    from {
      id
      displayName
    }
    content
  }
}"
                })
            };
            _transport.AddMessageToRead(expected);
            _transport.Complete();

            /* When */
            await _server.OnConnect();
            await _transport.Completion;

            /* Then */
            Assert.Empty(_server.Subscriptions);
            Assert.Contains(_transport.WrittenMessages,
                message => message.Type == MessageType.GQL_DATA);
            Assert.Contains(_transport.WrittenMessages,
                message => message.Type == MessageType.GQL_COMPLETE);
        }

        [Fact]
        public async Task Receive_start_query()
        {
            /* Given */
            var expected = new OperationMessage
            {
                Type = MessageType.GQL_START,
                Id = "1",
                Payload = JObject.FromObject(new OperationMessagePayload
                {
                    Query = @"{
  human() {
        name
        height
    }
}"
                })
            };
            _transport.AddMessageToRead(expected);
            _transport.Complete();

            /* When */
            await _server.OnConnect();
            await _transport.Completion;

            /* Then */
            Assert.Empty(_server.Subscriptions);
            Assert.Contains(_transport.WrittenMessages,
                message => message.Type == MessageType.GQL_DATA);
            Assert.Contains(_transport.WrittenMessages,
                message => message.Type == MessageType.GQL_COMPLETE);
        }

        [Fact(Skip = "server will actually unsubscribe when connection closed")]
        public async Task Receive_start_subscription()
        {
            /* Given */
            var source = new BufferBlock<ExecutionResult>();
            _executor.ExecuteAsync(null, null, null).ReturnsForAnyArgs(new SubscriptionResult(source, ()=> Task.CompletedTask));
            var expected = new OperationMessage
            {
                Type = MessageType.GQL_START,
                Id = "1",
                Payload = JObject.FromObject(new OperationMessagePayload
                {
                    Query = @"subscription MessageAdded {
  messageAdded {
    from { id displayName }
    content
  }
}"
                })
            };
            _transport.AddMessageToRead(expected);
            _transport.Complete();

            /* When */
            await _server.OnConnect();
            await _transport.Completion;

            /* Then */
            Assert.Single(_server.Subscriptions, sub => sub.Id == expected.Id);
        }

        [Fact]
        public async Task Receive_stop()
        {
            /* Given */
            var subscribe = new OperationMessage
            {
                Type = MessageType.GQL_START,
                Id = "1",
                Payload = JObject.FromObject(new OperationMessagePayload
                {
                    Query = "query"
                })
            };
            _transport.AddMessageToRead(subscribe);

            var unsubscribe = new OperationMessage
            {
                Type = MessageType.GQL_STOP,
                Id = "1"
            };
            _transport.AddMessageToRead(unsubscribe);
            _transport.Complete();

            /* When */
            await _server.OnConnect();
            await _transport.Completion;

            /* Then */
            Assert.Empty(_server.Subscriptions);
        }

        [Fact]
        public async Task Receive_terminate()
        {
            /* Given */
            var subscribe = new OperationMessage
            {
                Type = MessageType.GQL_CONNECTION_TERMINATE,
                Id = "1",
                Payload = JObject.FromObject(new OperationMessagePayload
                {
                    Query = "query"
                })
            };
            _transport.AddMessageToRead(subscribe);
            _transport.Complete();

            /* When */
            await _server.OnConnect();
            await _transport.Completion;

            /* Then */
            Assert.Empty(_server.Subscriptions);
        }

        [Fact]
        public async Task Receive_unknown()
        {
            /* Given */
            var expected = new OperationMessage
            {
                Type = "x"
            };
            _transport.AddMessageToRead(expected);
            _transport.Complete();

            /* When */
            await _server.OnConnect();
            await _transport.Completion;

            /* Then */
            Assert.Contains(_transport.WrittenMessages,
                message => message.Type == MessageType.GQL_CONNECTION_ERROR);
        }
    }
}