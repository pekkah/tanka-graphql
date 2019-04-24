using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Newtonsoft.Json;
using tanka.graphql.server.webSockets.dtos;

namespace tanka.graphql.server.webSockets
{
    public class GraphQLServer
    {
        private readonly IDuplexPipe _connection;
        private readonly Channel<OperationMessage> _readChannel;
        private readonly Channel<OperationMessage> _writeChannel;

        public GraphQLServer(IDuplexPipe connection)
        {
            _connection = connection;
            _readChannel = Channel.CreateUnbounded<OperationMessage>();
            _writeChannel = Channel.CreateUnbounded<OperationMessage>();
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

        public async Task RunAsync(CancellationToken token)
        {
            var toConnection = WriteToConnection(
                _connection.Output,
                token);

            var fromConnection = ReadFromConnection(
                _connection.Input,
                _readChannel.Writer,
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
                output.OnReaderCompleted((err,state) =>
                {
                    _writeChannel.Writer.TryComplete(err);
                }, null);

                while (true)
                {
                    if (!await reader.WaitToReadAsync(token))
                        break;

                    if (!reader.TryRead(out var message)) 
                        continue;
                    
                    var memory = output.GetMemory();
                    var count = WriteOperationMessage(message, memory.Span);
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

        private static async Task ReadFromConnection(PipeReader reader, ChannelWriter<OperationMessage> writer,
            CancellationToken token)
        {
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

        private static int WriteOperationMessage(OperationMessage message, Span<byte> span)
        {
            var json = JsonConvert.SerializeObject(message);
            json += '\n';
            return Encoding.UTF8.GetBytes(json, span);
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