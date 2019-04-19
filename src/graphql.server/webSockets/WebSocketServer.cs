using System.Buffers;
using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace tanka.graphql.server.webSockets
{
    public class WebSocketServer
    {
        private readonly WebSocketConnection _connection;
        private readonly ITextProtocolApplication _application;

        public WebSocketServer(WebSocketConnection connection, ITextProtocolApplication application)
        {
            _connection = connection;
            _application = application;
        }

        public Task ProcessRequestAsync(HttpContext context, CancellationToken token)
        {
            var receiveMessages = ReceiveMessages();
            var processRequest = _connection.ProcessRequestAsync(context, token);

            return Task.WhenAll(receiveMessages, processRequest);
        }

        protected async Task ReceiveMessages()
        {
            var reader = _connection.Input;
            while (true)
            {
                ReadResult result = await reader.ReadAsync();
                ReadOnlySequence<byte> buffer = result.Buffer;

                var message = Encoding.UTF8.GetString(result.Buffer.First.Span);
                _application.OnMessage(message);

                // Tell the PipeReader how much of the buffer we have consumed
                reader.AdvanceTo(buffer.Start, buffer.End);

                // Stop reading if there's no more data coming
                if (result.IsCompleted)
                {
                    break;
                }
            }

            // Mark the PipeReader as complete
            reader.Complete();
        }
    }
}