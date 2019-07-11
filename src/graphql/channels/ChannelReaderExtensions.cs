using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace tanka.graphql.channels
{
    public static class ChannelReaderExtensions
    {
        /// <summary>
        ///     Read items from reader and write to writer
        /// </summary>
        /// <typeparam name="T">Type of the item</typeparam>
        /// <param name="reader">Read channel</param>
        /// <param name="writer">Write channel</param>
        /// <returns></returns>
        public static async Task WriteTo<T>(
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

        /// <summary>
        ///     Read items from reader, transform and write to writer
        ///     and optionally complete writer when reader completes
        ///     or errors.
        /// </summary>
        /// <typeparam name="TSource">Type of the item</typeparam>
        /// <typeparam name="TTarget">Type of the target item</typeparam>
        /// <param name="reader">Read channel</param>
        /// <param name="writer">Write channel</param>
        /// <param name="transformAsync"></param>
        /// <param name="completeOnReaderCompletion">Complete writer on reader</param>
        /// <returns></returns>
        /// <returns></returns>
        public static async Task TransformAndWriteTo<TSource, TTarget>(
            this ChannelReader<TSource> reader,
            ChannelWriter<TTarget> writer,
            Func<TSource, Task<TTarget>> transformAsync,
            bool completeOnReaderCompletion = true)
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
                if (completeOnReaderCompletion)
                    writer.TryComplete(ex);

                throw;
            }
            finally
            {
                // This will safely no-op if the catch block above ran.
                if(completeOnReaderCompletion)
                    writer.TryComplete();
            }
        }

        /// <summary>
        ///     Read data from reader, transform and optionally complete
        ///     writer when reader competes or errors.
        /// </summary>
        /// <typeparam name="TSource">Type of source item</typeparam>
        /// <typeparam name="TTarget">Type of target item</typeparam>
        /// <param name="reader"></param>
        /// <param name="writer"></param>
        /// <param name="transform"></param>
        /// <param name="completeOnReaderCompletion">Complete writer on reader</param>
        /// <returns></returns>
        public static async Task TransformAndWriteTo<TSource, TTarget>(
            this ChannelReader<TSource> reader,
            ChannelWriter<TTarget> writer,
            Func<TSource, TTarget> transform,
            bool completeOnReaderCompletion = true)
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
                if (completeOnReaderCompletion)
                    writer.TryComplete(ex);

                throw;
            }
            finally
            {
                // This will safely no-op if the catch block above ran.
                if(completeOnReaderCompletion)
                    writer.TryComplete();
            }
        }
    }
}