using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using tanka.graphql.server.webSockets.dtos;

namespace tanka.graphql.server.webSockets
{
    public class WebSocketServer
    {
        private readonly ILoggerFactory _loggerFactory;
        private ILogger<WebSocketServer> _logger;

        public Channel<OperationMessage>
            Messages = System.Threading.Channels.Channel.CreateUnbounded<OperationMessage>();

        public WebSocketServer(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<WebSocketServer>();
        }

        public Task ProcessRequestAsync(HttpContext context)
        {
            var connection = new WebSocketConnection(_loggerFactory);
            var channel = Messages; //todo: should create per connection
            var reader = Task.Run(() => TransformToMessages(connection.Input, channel, context.RequestAborted));

            return Task.WhenAll(
                connection.ProcessRequestAsync(context),
                reader);
        }

        private static async Task TransformToMessages(PipeReader reader, ChannelWriter<OperationMessage> writer,
            CancellationToken token)
        {
            while (true)
            {
                var read = await reader.ReadAsync(token);
                if (read.IsCanceled)
                    break;

                // can we find a complete frame?
                var buffer = read.Buffer;
                if (TryParseFrame(
                    buffer,
                    out var nextMessage,
                    out var consumedTo))
                {
                    reader.AdvanceTo(consumedTo);
                    await writer.WriteAsync(nextMessage, token);
                    continue;
                }

                reader.AdvanceTo(buffer.Start, buffer.End);

                if (read.IsCompleted)
                    break;
            }
        }

        private static bool TryParseFrame(
            ReadOnlySequence<byte> buffer,
            out OperationMessage nextMessage,
            out SequencePosition consumedTo)
        {
            // find the end-of-line marker
            var eol = buffer.PositionOf((byte) '\n');
            if (eol == null)
            {
                nextMessage = default;
                consumedTo = default;
                return false;
            }

            // read past the line-ending
            consumedTo = buffer.GetPosition(1, eol.Value);
            // consume the data
            var payload = buffer.Slice(0, eol.Value);
            nextMessage = ReadOperationMessage(payload);
            return true;
        }

        private static OperationMessage ReadOperationMessage(in ReadOnlySequence<byte> payload)
        {
            return JsonConvert.DeserializeObject<OperationMessage>(GetUtf8String(payload));
        }

        private static string GetUtf8String(ReadOnlySequence<byte> buffer)
        {
            if (buffer.IsSingleSegment) return Encoding.UTF8.GetString(buffer.First.Span);

            return string.Create((int) buffer.Length, buffer, (span, sequence) =>
            {
                foreach (var segment in sequence)
                {
                    Encoding.UTF8.GetChars(segment.Span, span);

                    span = span.Slice(segment.Length);
                }
            });
        }
    }
}