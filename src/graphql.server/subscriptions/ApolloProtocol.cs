using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace fugu.graphql.server.subscriptions
{
    public class ApolloProtocol : IOperationMessageListener
    {
        private readonly ILogger<ApolloProtocol> _logger;

        public ApolloProtocol(ILogger<ApolloProtocol> logger)
        {
            _logger = logger;
        }


        public Task BeforeHandleAsync(MessageHandlingContext context)
        {
            return Task.CompletedTask;
        }

        public async Task HandleAsync(MessageHandlingContext context)
        {
            if (context.Terminated)
                return;

            var message = context.Message;
            switch (message.Type)
            {
                case MessageType.GQL_CONNECTION_INIT:
                    await HandleInitAsync(context);
                    break;
                case MessageType.GQL_START:
                    await HandleStartAsync(context);
                    break;
                case MessageType.GQL_STOP:
                    await HandleStopAsync(context);
                    break;
                case MessageType.GQL_CONNECTION_TERMINATE:
                    await HandleTerminateAsync(context);
                    break;
                default:
                    await HandleUnknownAsync(context);
                    break;
            }
        }

        public Task AfterHandleAsync(MessageHandlingContext context)
        {
            return Task.CompletedTask;
        }

        private Task HandleUnknownAsync(MessageHandlingContext context)
        {
            var message = context.Message;
            _logger.LogError($"Unexpected message type: {message.Type}");
            return context.Writer.SendAsync(new OperationMessage
            {
                Type = MessageType.GQL_CONNECTION_ERROR,
                Id = message.Id,
                Payload = JObject.FromObject(new
                {
                    message.Id,
                    Errors = new []
                    {
                        new Error($"Unexpected message type {message.Type}")
                    }
                })
            });
        }

        private Task HandleStopAsync(MessageHandlingContext context)
        {
            var message = context.Message;
            _logger.LogInformation("Handle stop: {id}", message.Id);
            return context.Subscriptions.UnsubscribeAsync(message.Id);
        }

        private Task HandleStartAsync(MessageHandlingContext context)
        {
            var message = context.Message;
            _logger.LogInformation("Handle start: {id}", message.Id);
            var payload = message.Payload.ToObject<OperationMessagePayload>();
            if (payload == null)
                throw new InvalidOperationException($"Could not get OperationMessagePayload from message.Payload");

            payload.Variables = payload.Variables;
            return context.Subscriptions.SubscribeOrExecuteAsync(
                message.Id,
                payload,
                context);
        }

        private Task HandleInitAsync(MessageHandlingContext context)
        {
            _logger.LogInformation("Handle init");
            return context.Writer.SendAsync(new OperationMessage
            {
                Type = MessageType.GQL_CONNECTION_ACK
            });
        }

        private async Task HandleTerminateAsync(MessageHandlingContext context)
        {
            _logger.LogInformation("Handle terminate");
            context.Reader.Complete();
            await context.Reader.Completion;
        }
    }
}