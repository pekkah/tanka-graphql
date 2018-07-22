using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;

namespace fugu.graphql.server.subscriptions
{
    /// <summary>
    ///     Subscription server
    ///     Acts as a message pump reading, handling and writing messages
    /// </summary>
    public class SubscriptionServer : IServerOperations
    {
        private readonly ILogger<SubscriptionServer> _logger;
        private readonly IEnumerable<IOperationMessageListener> _messageListeners;
        private ActionBlock<OperationMessage> _handler;

        public SubscriptionServer(
            IMessageTransport transport,
            ISubscriptionManager subscriptions,
            IEnumerable<IOperationMessageListener> messageListeners,
            ILogger<SubscriptionServer> logger)
        {
            _messageListeners = messageListeners;
            _logger = logger;
            Subscriptions = subscriptions;
            Transport = transport;
        }

        public IMessageTransport Transport { get; }

        public ISubscriptionManager Subscriptions { get; }

        public async Task OnConnect()
        {
            _logger.LogInformation("Connected...");
            LinkToTransportReader();

            LogServerInformation();

            // when transport reader is completed it should propagate here
            await _handler.Completion;

            await Subscriptions.UnsubscribeAllAsync();

            // complete write buffer
            Transport.Writer.Complete();
            await Transport.Completion;
        }

        private void LogServerInformation()
        {
            // list listeners
            var builder = new StringBuilder();
            builder.AppendLine("Message listeners:");
            foreach (var listener in _messageListeners)
                builder.AppendLine(listener.GetType().FullName);

            _logger.LogInformation(builder.ToString());
        }

        private void LinkToTransportReader()
        {
            _logger.LogDebug("Creating reader pipeline");
            _handler = new ActionBlock<OperationMessage>(HandleMessageAsync, new ExecutionDataflowBlockOptions
            {
                EnsureOrdered = true,
                BoundedCapacity = 1
            });

            Transport.Reader.LinkTo(_handler, new DataflowLinkOptions
            {
                PropagateCompletion = true
            });

            _logger.LogDebug("Reader pipeline created");
        }

        private async Task HandleMessageAsync(OperationMessage message)
        {
            _logger.LogDebug("Handling message: {id} of type: {type}", message.Id, message.Type);
            using (var context = await BuildMessageHandlingContext(message))
            {
                await OnBeforeHandleAsync(context);

                if (context.Terminated)
                    return;

                await OnHandleAsync(context);
                await OnAfterHandleAsync(context);
            }
        }

        private async Task OnBeforeHandleAsync(MessageHandlingContext context)
        {
            foreach (var listener in _messageListeners) await listener.BeforeHandleAsync(context);
        }

        private Task<MessageHandlingContext> BuildMessageHandlingContext(OperationMessage message)
        {
            return Task.FromResult(new MessageHandlingContext(this, message));
        }

        private async Task OnHandleAsync(MessageHandlingContext context)
        {
            foreach (var listener in _messageListeners) await listener.HandleAsync(context);
        }

        private async Task OnAfterHandleAsync(MessageHandlingContext context)
        {
            foreach (var listener in _messageListeners) await listener.AfterHandleAsync(context);
        }
    }
}