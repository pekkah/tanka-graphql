using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using fugu.graphql.server.subscriptions.helpers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace fugu.graphql.server.subscriptions
{
    /// <inheritdoc />
    public class SubscriptionManager : ISubscriptionManager
    {
        private readonly IExecutor _executor;
        private readonly ILogger<SubscriptionManager> _logger;
        private readonly ILoggerFactory _loggerFactory;

        private readonly ConcurrentDictionary<string, Subscription> _subscriptions =
            new ConcurrentDictionary<string, Subscription>();

        public SubscriptionManager(IExecutor executor, ILoggerFactory loggerFactory)
        {
            _executor = executor;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<SubscriptionManager>();
        }

        public Subscription this[string id] => _subscriptions[id];

        public IEnumerator<Subscription> GetEnumerator()
        {
            return _subscriptions.Values.GetEnumerator();
        }

        /// <inheritdoc />
        public async Task SubscribeOrExecuteAsync(
            string id,
            OperationMessagePayload payload,
            MessageHandlingContext context)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (payload == null) throw new ArgumentNullException(nameof(payload));
            if (context == null) throw new ArgumentNullException(nameof(context));

            var subscription = await ExecuteAsync(id, payload, context);

            if (subscription == null)
                return;

            _subscriptions[id] = subscription;
        }

        /// <inheritdoc />
        public Task UnsubscribeAsync(string id)
        {
            if (_subscriptions.TryRemove(id, out var removed))
                return removed.UnsubscribeAsync();

            _logger.LogInformation("Subscription: {subcriptionId} unsubscribed", id);
            return Task.CompletedTask;
        }

        public async Task UnsubscribeAllAsync()
        {
            foreach (var subscription in _subscriptions)
            {
                await subscription.Value.UnsubscribeAsync();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _subscriptions.Values.GetEnumerator();
        }

        private async Task<Subscription> ExecuteAsync(
            string id,
            OperationMessagePayload payload,
            MessageHandlingContext context)
        {
            var writer = context.Writer;
            _logger.LogDebug("Executing operation: {operationName} query: {query}",
                payload.OperationName,
                payload.Query);

            var result = await _executor.ExecuteAsync(
                payload.Query,
                payload.OperationName,
                payload.Variables
            );

            if (result.Errors != null && result.Errors.Any())
            {
                _logger.LogError("Execution errors: {errors}", ResultHelper.GetErrorString(result));
                await writer.SendAsync(new OperationMessage
                {
                    Type = MessageType.GQL_ERROR,
                    Id = id,
                    Payload = JObject.FromObject(result)
                });

                await writer.SendAsync(new OperationMessage
                {
                    Type = MessageType.GQL_COMPLETE,
                    Id = id,
                    Payload = null
                });

                return null;
            }

            if (result is SubscriptionResult subscriptionResult)
            {
                if (subscriptionResult.Source == null)
                {
                    _logger.LogError("Cannot subscribe as no result stream available");
                    await writer.SendAsync(new OperationMessage
                    {
                        Type = MessageType.GQL_ERROR,
                        Id = id,
                        Payload = JObject.FromObject(subscriptionResult)
                    });

                    return null;
                }

                _logger.LogInformation("Creating subscription");
                var subscription = new Subscription(
                    id,
                    writer,
                    subscriptionResult,
                    _loggerFactory.CreateLogger<Subscription>());

                // when subscription is completed remove from internal list
#pragma warning disable 4014
                subscription.Completion.ContinueWith(e => _subscriptions.TryRemove(id, out _));
#pragma warning restore 4014

                return subscription;
            }

            // must be mutation or query
            await writer.SendAsync(new OperationMessage
            {
                Type = MessageType.GQL_DATA,
                Id = id,
                Payload = JObject.FromObject(result)
            });

            await writer.SendAsync(new OperationMessage
            {
                Type = MessageType.GQL_COMPLETE,
                Id = id
            });

            return null;
        }
    }
}