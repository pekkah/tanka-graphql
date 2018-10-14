using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace fugu.graphql.server.subscriptions
{
    /// <summary>
    ///     Internal obsercer of the subsciption
    /// </summary>
    public class Subscription
    {
        private readonly ITargetBlock<OperationMessage> _writer;
        private readonly SubscriptionResult _subscriptionResult;
        private readonly ILogger<Subscription> _logger;
        public string Id { get; }
        private readonly TransformBlock<ExecutionResult, OperationMessage> _transformer;
        private TaskCompletionSource<object> _completionSource;

        public Subscription(
            string id, 
            ITargetBlock<OperationMessage> writer, 
            SubscriptionResult subscriptionResult,
            ILogger<Subscription> logger)
        {
            _writer = writer;
            _subscriptionResult = subscriptionResult;
            _logger = logger;
            _completionSource = new TaskCompletionSource<object>();

            Id = id;

            // setup transformer
            _transformer = new TransformBlock<ExecutionResult, OperationMessage>(
                er => ProcessResultAsync(er),
                new ExecutionDataflowBlockOptions
                {
                    EnsureOrdered = true,
                    MaxDegreeOfParallelism = 1
                });

            // link source to transformer
            subscriptionResult.Source.LinkTo(_transformer, new DataflowLinkOptions()
            {
                PropagateCompletion = true
            });


            // link transformer to write buffer
            _transformer.LinkTo(writer, new DataflowLinkOptions()
            {
                PropagateCompletion = false
            });

            _transformer.Completion.ContinueWith(Complete);
        }

        private void Complete(Task transformerCompletion)
        {
            var message = new OperationMessage()
            {
                Id = Id,
                Type = MessageType.GQL_COMPLETE
            };

            _writer.Post(message);
            _completionSource.SetResult(null);
            _logger.LogDebug("Subscription: {subscriptionId} completed", Id);
        }

        public Task Completion => _completionSource.Task;

        public async Task UnsubscribeAsync()
        {
            await _subscriptionResult.UnsubscribeAsync();
            await Completion;
            _logger.LogDebug("Subscription: {subscriptionId} unsubscribed", Id);
        }

        protected Task<OperationMessage> ProcessResultAsync(ExecutionResult er)
        {
            var message = new OperationMessage()
            {
                Id = Id,
                Type = MessageType.GQL_DATA,
                Payload = JObject.FromObject(er)
            };

            return Task.FromResult(message);
        }
    }
}