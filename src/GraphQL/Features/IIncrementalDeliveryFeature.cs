using System.Runtime.CompilerServices;

using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Features;

/// <summary>
/// Feature for tracking incremental delivery (@defer and @stream) during execution
/// </summary>
public interface IIncrementalDeliveryFeature
{
    /// <summary>
    /// Whether this execution has any deferred or streamed work
    /// </summary>
    bool HasIncrementalWork { get; }

    /// <summary>
    /// Register a deferred fragment for later execution
    /// </summary>
    void RegisterDeferredWork(string? label, NodePath path, Func<Task<IncrementalPayload>> executionFunc);

    /// <summary>
    /// Register streaming deferred work for later execution
    /// </summary>
    void RegisterDeferredStream(string? label, NodePath path, Func<IAsyncEnumerable<IncrementalPayload>> streamFunc);

    /// <summary>
    /// Get all pending deferred work as an async enumerable
    /// </summary>
    IAsyncEnumerable<IncrementalPayload> GetDeferredResults(CancellationToken cancellationToken);

    /// <summary>
    /// Complete the incremental delivery (no more deferred work will be registered)
    /// </summary>
    void Complete();
}

/// <summary>
/// Default implementation of IIncrementalDeliveryFeature
/// </summary>
public class DefaultIncrementalDeliveryFeature : IIncrementalDeliveryFeature
{
    private readonly List<(string? Label, NodePath Path, Func<IAsyncEnumerable<IncrementalPayload>> StreamFunc)> _deferredStreams = new();
    private readonly object _lock = new();
    private volatile bool _isCompleted;

    public DefaultIncrementalDeliveryFeature()
    {
    }

    public bool HasIncrementalWork { get; private set; }

    public void RegisterDeferredWork(string? label, NodePath path, Func<Task<IncrementalPayload>> executionFunc)
    {
        if (_isCompleted)
            throw new InvalidOperationException("Cannot register deferred work after completion");

        lock (_lock)
        {
            HasIncrementalWork = true;
            // Wrap single payload function as async enumerable
            _deferredStreams.Add((label, path, () => WrapSinglePayload(executionFunc)));
        }
    }

    public void RegisterDeferredStream(string? label, NodePath path, Func<IAsyncEnumerable<IncrementalPayload>> streamFunc)
    {
        if (_isCompleted)
            throw new InvalidOperationException("Cannot register deferred stream after completion");

        lock (_lock)
        {
            HasIncrementalWork = true;
            _deferredStreams.Add((label, path, streamFunc));
        }
    }

    public async IAsyncEnumerable<IncrementalPayload> GetDeferredResults([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var activeStreams = new List<(string? Label, NodePath Path, IAsyncEnumerator<IncrementalPayload> Enumerator)>();
        var streamTasks = new HashSet<Task<(int StreamIndex, IncrementalPayload? Payload, bool HasMore)>>();

        // Get snapshot of registered streams
        (string? Label, NodePath Path, Func<IAsyncEnumerable<IncrementalPayload>> StreamFunc)[] streamData;
        lock (_lock)
        {
            streamData = _deferredStreams.ToArray();
        }

        try
        {
            // Start all streams and get their first items
            for (int streamIndex = 0; streamIndex < streamData.Length; streamIndex++)
            {
                var (label, path, streamFunc) = streamData[streamIndex];
                var stream = streamFunc();
                var enumerator = stream.GetAsyncEnumerator(cancellationToken);
                activeStreams.Add((label, path, enumerator));
                
                // Start reading from this stream
                streamTasks.Add(ReadNextFromStream(streamIndex, path, enumerator, cancellationToken));
            }

            // Process payloads as they arrive from any stream
            while (streamTasks.Count > 0)
            {
                var completedTask = await Task.WhenAny(streamTasks);
                streamTasks.Remove(completedTask);

                var (completedStreamIndex, payload, hasMore) = await completedTask;

                // Yield the payload if we got one
                if (payload != null)
                    yield return payload;

                // If this stream has more items, start reading the next one
                if (hasMore)
                {
                    var (_, path, enumerator) = activeStreams[completedStreamIndex];
                    streamTasks.Add(ReadNextFromStream(completedStreamIndex, path, enumerator, cancellationToken));
                }
            }
        }
        finally
        {
            // Clean up all enumerators
            foreach (var (_, _, enumerator) in activeStreams)
            {
                await enumerator.DisposeAsync();
            }
        }
    }

    private static async Task<(int StreamIndex, IncrementalPayload? Payload, bool HasMore)> ReadNextFromStream(
        int streamIndex, 
        NodePath path,
        IAsyncEnumerator<IncrementalPayload> enumerator, 
        CancellationToken cancellationToken)
    {
        try
        {
            if (await enumerator.MoveNextAsync())
            {
                return (streamIndex, enumerator.Current, true);
            }
            else
            {
                return (streamIndex, null, false);
            }
        }
        catch (OperationCanceledException)
        {
            return (streamIndex, null, false);
        }
        catch (Exception ex)
        {
            // Create an error payload for stream failures with proper path information
            var errorPayload = new IncrementalPayload
            {
                Path = path,
                Errors = new[] { new ExecutionError { Message = ex.Message, Path = path.Segments.ToArray() } }
            };
            return (streamIndex, errorPayload, false);
        }
    }

    public void Complete()
    {
        _isCompleted = true;
        // No cleanup needed - just mark as completed
    }

    private static async IAsyncEnumerable<IncrementalPayload> WrapSinglePayload(Func<Task<IncrementalPayload>> executionFunc)
    {
        var result = await executionFunc();
        yield return result;
    }
}