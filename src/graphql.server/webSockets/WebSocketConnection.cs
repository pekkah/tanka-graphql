using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.Extensions.Logging;

namespace tanka.graphql.server.webSockets
{
    public class WebSocketConnection : IDuplexPipe
    {
        private readonly Pipe _pipe;
        private readonly WebSocketsTransport _transport;

        public WebSocketConnection(ILoggerFactory loggerFactory)
        {
            _pipe = new Pipe();
            _transport = new WebSocketsTransport(new WebSocketOptions(), this, loggerFactory);
        }

        public PipeReader Input => _pipe.Reader;

        public PipeWriter Output => _pipe.Writer;

        public Task ProcessRequestAsync(HttpContext context, CancellationToken token)
        {
            return _transport.ProcessRequestAsync(context, token);
        }
    }
}