using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using fugu.graphql.server.subscriptions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace fugu.graphql.server.tests.subscriptions
{
    public class SubscriptionManagerFacts
    {
        public SubscriptionManagerFacts()
        {
            _writer = Substitute.For<ITargetBlock<OperationMessage>>();
            _reader = Substitute.For<ISourceBlock<OperationMessage>>();
            _transport = Substitute.For<IMessageTransport>();
            _transport.Reader.Returns(_reader);
            _transport.Writer.Returns(_writer);

            _executer = Substitute.For<IExecutor>();
            _sut = new SubscriptionManager(_executer, new NullLoggerFactory());
            _server = new TestableServerOperations(_transport, _sut);
        }

        private readonly SubscriptionManager _sut;
        private readonly IServerOperations _server;
        private readonly ITargetBlock<OperationMessage> _writer;
        private readonly ISourceBlock<OperationMessage> _reader;
        private readonly IExecutor _executer;
        private readonly IMessageTransport _transport;

        [Fact]
        public async Task Failed_Subscribe_does_not_add()
        {
            /* Given */
            var id = "1";
            var payload = new OperationMessagePayload();
            var context = new MessageHandlingContext(_server, null);

            _executer.ExecuteAsync(null, null, null).ReturnsForAnyArgs(
                new SubscriptionResult
                {
                    Errors = new[]
                    {
                        new Error("error")
                    }
                });

            /* When */
            await _sut.SubscribeOrExecuteAsync(id, payload, context);

            /* Then */
            Assert.Empty(_sut);
        }

        [Fact]
        public async Task Failed_Subscribe_with_null_stream()
        {
            /* Given */
            var id = "1";
            var payload = new OperationMessagePayload();
            var context = new MessageHandlingContext(_server, null);

            _executer.ExecuteAsync(null, null, null).ReturnsForAnyArgs(
                new SubscriptionResult(null, null));

            /* When */
            await _sut.SubscribeOrExecuteAsync(id, payload, context);

            /* Then */
            await _writer.Received().SendAsync(
                Arg.Is<OperationMessage>(
                    message => message.Id == id
                               && message.Type == MessageType.GQL_ERROR));
        }

        [Fact]
        public async Task Failed_Subscribe_writes_error()
        {
            /* Given */
            var id = "1";
            var payload = new OperationMessagePayload();
            var context = new MessageHandlingContext(_server, null);

            _executer.ExecuteAsync(null, null, null).ReturnsForAnyArgs(
                new SubscriptionResult
                {
                    Errors = new[]
                    {
                        new Error("error")
                    }
                });

            /* When */
            await _sut.SubscribeOrExecuteAsync(id, payload, context);

            /* Then */
            await _writer.Received().SendAsync(
                Arg.Is<OperationMessage>(
                    message => message.Id == id
                               && message.Type == MessageType.GQL_ERROR));
        }

        [Fact]
        public async Task Subscribe_adds()
        {
            /* Given */
            var source = new BufferBlock<ExecutionResult>();
            _executer.ExecuteAsync(null, null, null).ReturnsForAnyArgs(
                new SubscriptionResult(source, () => Task.CompletedTask));

            var id = "1";
            var payload = new OperationMessagePayload();
            var context = new MessageHandlingContext(_server, null);

            /* When */
            await _sut.SubscribeOrExecuteAsync(id, payload, context);

            /* Then */
            Assert.Single(_sut, sub => sub.Id == id);
        }

        [Fact]
        public async Task Subscribe_executes()
        {
            /* Given */
            var id = "1";
            var payload = new OperationMessagePayload();
            var context = new MessageHandlingContext(_server, null);

            /* When */
            await _sut.SubscribeOrExecuteAsync(id, payload, context);

            /* Then */
            await _executer.Received().ExecuteAsync(
                Arg.Is(payload.OperationName),
                Arg.Is(payload.Query),
                Arg.Any<dynamic>());
        }

        [Fact]
        public async Task Unsubscribe_removes()
        {
            /* Given */
            var id = "1";
            var payload = new OperationMessagePayload();
            var context = new MessageHandlingContext(_server, null);

            await _sut.SubscribeOrExecuteAsync(id, payload, context);

            /* When */
            await _sut.UnsubscribeAsync(id);

            /* Then */
            Assert.Empty(_sut);
        }

        [Fact]
        public async Task Unsubscribe_writes_complete()
        {
            /* Given */
            var id = "1";
            var payload = new OperationMessagePayload();
            var context = new MessageHandlingContext(_server, null);

            await _sut.SubscribeOrExecuteAsync(id, payload, context);

            /* When */
            await _sut.UnsubscribeAsync(id);

            /* Then */
            await _writer.Received().SendAsync(
                Arg.Is<OperationMessage>(
                    message => message.Id == id
                               && message.Type == MessageType.GQL_COMPLETE));
        }
    }
}