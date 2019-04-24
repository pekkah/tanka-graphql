using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace tanka.graphql.server.webSockets
{
    public class SubscriptionServer : MessageServer
    {
        private readonly IProtocolHandler _protocol;

        public SubscriptionServer(
            IProtocolHandler protocol)
        {
            _protocol = protocol;
        }

        public override Task RunAsync(IDuplexPipe connection, CancellationToken token)
        {
            var baseRun = base.RunAsync(connection, token);
            var receiveOperations = StartReceiveOperations();

            return Task.WhenAll(receiveOperations, baseRun);
        }

        private async Task StartReceiveOperations()
        {
            while (await Input.WaitToReadAsync())
            while (Input.TryRead(out var operation))
                await _protocol.Handle(new MessageContext(operation, Output));
        }
    }
}