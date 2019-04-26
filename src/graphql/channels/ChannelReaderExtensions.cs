using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace tanka.graphql.channels
{
    public static class ChannelReaderExtensions
    {
        public static async Task LinkTo<T>(
            this ChannelReader<T> reader,
            ChannelWriter<T> writer)
        {
            try
            {
                while (await reader.WaitToReadAsync())
                while (reader.TryRead(out var evnt))
                while (!writer.TryWrite(evnt))
                    if (!await writer.WaitToWriteAsync())
                        return;

                // Manifest any errors in the completion task
                await reader.Completion;
            }
            catch (Exception ex)
            {
                writer.TryComplete(ex);
            }
            finally
            {
                // This will safely no-op if the catch block above ran.
                writer.TryComplete();
            }
        }

        public static async Task TransformAndLinkTo<TSource, TTarget>(
            this ChannelReader<TSource> reader,
            ChannelWriter<TTarget> writer,
            Func<TSource, Task<TTarget>> transformAsync)
        {
            try
            {
                while (await reader.WaitToReadAsync())
                while (reader.TryRead(out var evnt))
                {
                    var executionResult = await transformAsync(evnt);

                    while (!writer.TryWrite(executionResult))
                        if (!await writer.WaitToWriteAsync())
                            return;
                }

                // Manifest any errors in the completion task
                await reader.Completion;
            }
            catch (Exception ex)
            {
                writer.TryComplete(ex);
            }
            finally
            {
                // This will safely no-op if the catch block above ran.
                writer.TryComplete();
            }
        }

        public static async Task TransformAndLinkTo<TSource, TTarget>(
            this ChannelReader<TSource> reader,
            ChannelWriter<TTarget> writer,
            Func<TSource, TTarget> transform)
        {
            try
            {
                while (await reader.WaitToReadAsync())
                while (reader.TryRead(out var evnt))
                {
                    var executionResult = transform(evnt);

                    while (!writer.TryWrite(executionResult))
                        if (!await writer.WaitToWriteAsync())
                            return;
                }

                // Manifest any errors in the completion task
                await reader.Completion;
            }
            catch (Exception ex)
            {
                writer.TryComplete(ex);
            }
            finally
            {
                // This will safely no-op if the catch block above ran.
                writer.TryComplete();
            }
        }
    }
}