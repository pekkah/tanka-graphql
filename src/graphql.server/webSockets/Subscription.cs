using System.Threading;
using System.Threading.Channels;
using Tanka.GraphQL.Server.WebSockets.DTOs;

namespace Tanka.GraphQL.Server.WebSockets
{
    public class Subscription
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