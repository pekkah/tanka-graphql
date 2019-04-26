using System.Threading;
using System.Threading.Channels;
using tanka.graphql.server.webSockets.dtos;

namespace tanka.graphql.server.webSockets
{
    public struct Subscription
    {
        public Subscription(string id, QueryStream queryStream, ChannelWriter<OperationMessage> output,
            CancellationTokenSource cancellationTokenSource)
        {
            ID = id;
            QueryStream = queryStream;
            Unsubscribe = cancellationTokenSource;
        }

        public string ID { get; }
        public QueryStream QueryStream { get; set; }

        public CancellationTokenSource Unsubscribe { get; set; }
    }
}