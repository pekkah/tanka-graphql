using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using fugu.graphql.server.subscriptions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace fugu.graphql.server.tests.subscriptions
{
    public class SubscriptionServerFacts
    {
        public SubscriptionServerFacts()
        {
            _messageListener = Substitute.For<IOperationMessageListener>();
            _transport = new TestableSubscriptionTransport();

            _documentExecuter = Substitute.For<IExecutor>();
            _documentExecuter.ExecuteAsync(null, null, null).ReturnsForAnyArgs(
                new SubscriptionResult());
            _subscriptionManager = new SubscriptionManager(_documentExecuter, new NullLoggerFactory());
            _sut = new SubscriptionServer(
                _transport,
                _subscriptionManager,
                new[] {_messageListener},
                new NullLogger<SubscriptionServer>());
        }

        private readonly TestableSubscriptionTransport _transport;
        private readonly SubscriptionServer _sut;
        private readonly ISubscriptionManager _subscriptionManager;
        private readonly IExecutor _documentExecuter;
        private readonly IOperationMessageListener _messageListener;

        [Fact]
        public async Task Completion_order()
        {
            /* Given */


            /* When */
            _transport.Complete();
            await _sut.OnConnect();

            /* Then */
        }

        [Fact]
        public async Task Listener_BeforeHandle()
        {
            /* Given */
            var expected = new OperationMessage
            {
                Type = MessageType.GQL_CONNECTION_INIT
            };
            _transport.AddMessageToRead(expected);
            _transport.Complete();

            /* When */
            await _sut.OnConnect();
            await _transport.Completion;

            /* Then */
            await _messageListener.Received().BeforeHandleAsync(Arg.Is<MessageHandlingContext>(context =>
                context.Writer == _transport.Writer
                && context.Reader == _transport.Reader
                && context.Subscriptions == _subscriptionManager
                && context.Message == expected));
        }

        [Fact]
        public async Task Listener_Handle()
        {
            /* Given */
            var expected = new OperationMessage
            {
                Type = MessageType.GQL_CONNECTION_INIT
            };
            _transport.AddMessageToRead(expected);
            _transport.Complete();

            /* When */
            await _sut.OnConnect();
            await _transport.Completion;

            /* Then */
            await _messageListener.Received().HandleAsync(Arg.Is<MessageHandlingContext>(context =>
                context.Writer == _transport.Writer
                && context.Reader == _transport.Reader
                && context.Subscriptions == _subscriptionManager
                && context.Message == expected));
        }

        [Fact]
        public async Task Listener_AfterHandle()
        {
            /* Given */
            var expected = new OperationMessage
            {
                Type = MessageType.GQL_CONNECTION_INIT
            };
            _transport.AddMessageToRead(expected);
            _transport.Complete();

            /* When */
            await _sut.OnConnect();
            await _transport.Completion;

            /* Then */
            await _messageListener.Received().AfterHandleAsync(Arg.Is<MessageHandlingContext>(context =>
                context.Writer == _transport.Writer
                && context.Reader == _transport.Reader
                && context.Subscriptions == _subscriptionManager
                && context.Message == expected));
        }
    }
}