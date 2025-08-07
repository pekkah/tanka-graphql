using System.Runtime.CompilerServices;
using System.Threading.Channels;

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
    /// Get all pending deferred work as an async enumerable
    /// </summary>
    IAsyncEnumerable<IncrementalPayload> GetDeferredResults(CancellationToken cancellationToken);

    /// <summary>
    /// Add a stream item for later delivery
    /// </summary>
    void AddStreamItem(IncrementalPayload streamItem);

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
    private readonly Channel<Func<Task<IncrementalPayload>>> _deferredWork;
    private volatile bool _isCompleted;

    public DefaultIncrementalDeliveryFeature()
    {
        _deferredWork = Channel.CreateUnbounded<Func<Task<IncrementalPayload>>>();
    }

    public bool HasIncrementalWork { get; private set; }

    public void RegisterDeferredWork(string? label, NodePath path, Func<Task<IncrementalPayload>> executionFunc)
    {
        if (_isCompleted)
            throw new InvalidOperationException("Cannot register deferred work after completion");

        HasIncrementalWork = true;
        _deferredWork.Writer.TryWrite(executionFunc);
    }

    public void AddStreamItem(IncrementalPayload streamItem)
    {
        if (_isCompleted)
            throw new InvalidOperationException("Cannot add stream items after completion");

        HasIncrementalWork = true;
        _deferredWork.Writer.TryWrite(() => Task.FromResult(streamItem));
    }

    public async IAsyncEnumerable<IncrementalPayload> GetDeferredResults([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var workFunc in _deferredWork.Reader.ReadAllAsync(cancellationToken))
        {
            var result = await workFunc();
            yield return result;
        }
    }

    public void Complete()
    {
        _isCompleted = true;
        _deferredWork.Writer.TryComplete();
    }
}