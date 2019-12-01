using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Tanka.GraphQL.Server.WebSockets.DTOs;

namespace Tanka.GraphQL.Server.WebSockets
{
    public class MessageServer
    {
        private readonly Channel<OperationMessage> _readChannel;
        private readonly Channel<OperationMessage> _writeChannel;

        public MessageServer(IOptions<WebSocketServerOptions> options)
        {
            _readChannel = Channel.CreateUnbounded<OperationMessage>();
            _writeChannel = Channel.CreateUnbounded<OperationMessage>();
            _messageSerializerOptions = options.Value.MessageSerializerOptions;
        }

        public ChannelReader<OperationMessage> Input => _readChannel.Reader;

        public ChannelWriter<OperationMessage> Output => _writeChannel.Writer;

        public Task Completion => Task.WhenAll(
            _readChannel.Reader.Completion,
            _writeChannel.Reader.Completion);

        public void Complete(Exception ex = null)
        {
            _writeChannel.Writer.TryComplete(ex);
            _readChannel.Writer.TryComplete(ex);
        }

        public virtual async Task RunAsync(IDuplexPipe connection, CancellationToken token)
        {
            var toConnection = WriteToConnection(
                connection.Output,
                token);

            var fromConnection = ReadFromConnection(
                connection.Input,
                token);

            await Task.WhenAll(
                toConnection,
                fromConnection,
                Completion);
        }

        private async Task WriteToConnection(
            PipeWriter output,
            CancellationToken token)
        {
            try
            {
                var reader = _writeChannel.Reader;
                while (true)
                {
                    if (!await reader.WaitToReadAsync(token))
                        break;

                    if (!reader.TryRead(out var message))
                        continue;

                    var count = WriteOperationMessage(message, output);
                    output.Advance(count);

                    // apply back-pressure etc
                    var flush = await output.FlushAsync(token);

                    if (flush.IsCanceled || flush.IsCompleted)
                        break;
                }

                // Manifest any errors in the completion task
                await reader.Completion;
            }
            catch (Exception ex)
            {
                if (token.IsCancellationRequested)
                    return;

                output.Complete(ex);
                throw;
            }
            finally
            {
                // This will safely no-op if the catch block above ran.
                output.Complete();
            }
        }

        private async Task ReadFromConnection(
            PipeReader reader,
            CancellationToken token)
        {
            var writer = _readChannel.Writer;

            try
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
            catch (Exception e)
            {
                if (token.IsCancellationRequested)
                    return;

                reader.Complete(e);
                writer.TryComplete(e);
                throw;
            }
            finally
            {
                reader.Complete();
                writer.TryComplete();
            }
        }

        private bool TryParseFrame(
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

        private static readonly byte[] _separatorBytes = Encoding.UTF8.GetBytes(new[] {'\n'});
        private readonly JsonSerializerOptions _messageSerializerOptions;

        private  int WriteOperationMessage(OperationMessage message, PipeWriter output)
        {
            var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(message, _messageSerializerOptions);
            
            byte[] messageBytes = new byte[jsonBytes.Length + 1];
            jsonBytes.CopyTo(messageBytes, 0);
            _separatorBytes.CopyTo(messageBytes, jsonBytes.Length);

            var memory = output.GetMemory(sizeHint: messageBytes.Length);
            messageBytes.CopyTo(memory);
            return messageBytes.Length;
        }

        private OperationMessage ReadOperationMessage(in ReadOnlySequence<byte> payload)
        {
            return JsonSerializer.Deserialize<OperationMessage>(payload.ToArray(), _messageSerializerOptions);
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