using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using fugu.graphql.server.subscriptions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace fugu.graphql.server.tests.subscriptions
{
    public class SubscriptionFacts
    {
        public SubscriptionFacts()
        {
            _transport = new TestableSubscriptionTransport();
            _writer = _transport.Writer;
        }

        private readonly ITargetBlock<OperationMessage> _writer;
        private readonly TestableSubscriptionTransport _transport;

        [Fact]
        public async Task On_data_from_stream()
        {
            /* Given */
            var id = "1";
            var payload = new OperationMessagePayload();
            var source = new BufferBlock<ExecutionResult>();
            var result = new SubscriptionResult(
                source,
                () =>
                {
                    source.Complete();
                    return source.Completion;
                });

            var sut = new Subscription(id, _writer, result, new NullLogger<Subscription>());

            /* When */
            await source.SendAsync(new ExecutionResult());
            source.Complete();
            await source.Completion;
            await sut.Completion;

            // wait writer to finish
            _writer.Complete();
            await _writer.Completion;

            /* Then */
            Assert.Single(_transport.WrittenMessages,
                message => message.Id == id && message.Type == MessageType.GQL_DATA);
        }

        [Fact]
        public async Task On_stream_complete()
        {
            /* Given */
            var id = "1";
            var payload = new OperationMessagePayload();
            var source = new BufferBlock<ExecutionResult>();
            var result = new SubscriptionResult(
                source,
                () =>
                {
                    source.Complete();
                    return source.Completion;
                });

            var completed = Substitute.For<Action<Subscription>>();
            var sut = new Subscription(id, _writer, result, new NullLogger<Subscription>());

            /* When */
            source.Complete();
            await source.Completion;
            await sut.Completion;


            /* Then */
            Assert.Single(_transport.WrittenMessages,
                message => message.Id == id && message.Type == MessageType.GQL_COMPLETE);
        }

        [Fact]
        public async Task Subscribe_to_completed_stream_should_not_throw()
        {
            /* Given */
            var id = "1";
            var payload = new OperationMessagePayload();
            var source = new BufferBlock<ExecutionResult>();
            source.Complete();
            await source.Completion;

            var result = new SubscriptionResult(
                source,
                () =>
                {
                    source.Complete();
                    return source.Completion;
                });

            /* When */
            /* Then */
            var sut = new Subscription(id, _writer, result, new NullLogger<Subscription>());
        }

        [Fact]
        public async Task Subscribe_to_stream()
        {
            /* Given */
            var id = "1";
            var payload = new OperationMessagePayload();
            var source = new BufferBlock<ExecutionResult>();
            var result = new SubscriptionResult(
                source,
                () =>
                {
                    source.Complete();
                    return source.Completion;
                });

            /* When */
            var sut = new Subscription(id, _writer, result, new NullLogger<Subscription>());
            source.Complete();
            await sut.Completion;

            /* Then */
            //todo:??
        }

        [Fact]
        public async Task Unsubscribe_from_stream()
        {
            /* Given */
            var id = "1";
            var payload = new OperationMessagePayload();
            var source = new BufferBlock<ExecutionResult>();
            var result = new SubscriptionResult(
                source,
                () =>
                {
                    source.Complete();
                    return source.Completion;
                });

            var sut = new Subscription(id, _writer, result, new NullLogger<Subscription>());

            /* When */
            await sut.UnsubscribeAsync();


            /* Then */
            //todo??
        }

        [Fact]
        public async Task Write_Complete_on_unsubscribe()
        {
            /* Given */
            var id = "1";
            var payload = new OperationMessagePayload();
            var source = new BufferBlock<ExecutionResult>();
            var result = new SubscriptionResult(
                source,
                () =>
                {
                    source.Complete();
                    return source.Completion;
                });

            var sut = new Subscription(id, _writer, result, new NullLogger<Subscription>());

            /* When */
            await sut.UnsubscribeAsync();
            _writer.Complete();
            await _writer.Completion;


            /* Then */
            Assert.Single(_transport.WrittenMessages,
                message => message.Id == id && message.Type == MessageType.GQL_COMPLETE);
        }
    }
}