using System.Runtime.CompilerServices;

using Tanka.GraphQL.Features;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.SelectionSets;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL;

public partial class Executor
{
    /// <summary>
    ///     Static method to execute subscription operation using given <paramref name="context" />
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static Task ExecuteSubscription(QueryContext context)
    {
        context.RequestCancelled.ThrowIfCancellationRequested();

        IAsyncEnumerable<object?> sourceStream = CreateSourceEventStream(
            context,
            context.RequestCancelled);

        IAsyncEnumerable<ExecutionResult> responseStream = MapSourceToResponseEventStream(
            context,
            sourceStream,
            context.RequestCancelled);

        context.Response = responseStream;
        return Task.CompletedTask;
    }


    /// <summary>
    ///     Create subscription source event stream for given <paramref name="context" />.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="QueryException"></exception>
    public static async IAsyncEnumerable<object?> CreateSourceEventStream(
        QueryContext context,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context.Schema.Subscription);

        ObjectDefinition? subscriptionType = context.Schema.Subscription;
        IReadOnlyDictionary<string, List<FieldSelection>> groupedFieldSet = FieldCollector.CollectFields(
            context.Schema,
            context.Request.Document,
            subscriptionType,
            context.OperationDefinition.SelectionSet,
            context.CoercedVariableValues
        );

        List<FieldSelection> fields = groupedFieldSet.Values.First();
        Name fieldName = fields.First().Name;
        FieldSelection fieldSelection = fields.First();

        IReadOnlyDictionary<string, object?> coercedArgumentValues = ArgumentCoercion.CoerceArgumentValues(
            context.Schema,
            subscriptionType,
            fieldSelection,
            context.CoercedVariableValues);

        FieldDefinition? field = context.Schema.GetField(subscriptionType.Name, fieldName);

        if (field is null)
            yield break;

        var path = new NodePath();
        Subscriber? subscriber = context.Schema.GetSubscriber(subscriptionType.Name, fieldName);

        if (subscriber == null)
            throw new QueryException(
                $"Could not subscribe. Field '{subscriptionType}:{fieldName}' does not have subscriber")
            {
                Path = path
            };

        var resolverContext = new SubscriberContext
        {
            ArgumentValues = coercedArgumentValues,
            Field = field,
            Fields = fields,
            ObjectDefinition = subscriptionType,
            ObjectValue = context.Request.InitialValue,
            Path = path,
            Selection = fieldSelection,
            QueryContext = context
        };

        await subscriber(resolverContext, cancellationToken);

        if (resolverContext.ResolvedValue is null)
            yield break;

        await foreach (object? evnt in resolverContext.ResolvedValue.WithCancellation(cancellationToken))
            yield return evnt;
    }

    /// <summary>
    ///     Map subscription source event stream to response event stream for given <paramref name="context" />
    ///     and <paramref name="sourceStream" />.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="sourceStream"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async IAsyncEnumerable<ExecutionResult> MapSourceToResponseEventStream(
        QueryContext context,
        IAsyncEnumerable<object?> sourceStream,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context.Schema.Subscription);

        ObjectDefinition? subscriptionType = context.Schema.Subscription;
        SelectionSet selectionSet = context.OperationDefinition.SelectionSet;

        await foreach (object? sourceEvnt in sourceStream.WithCancellation(cancellationToken))
        {
            var path = new NodePath();
            yield return await ExecuteSourceEvent(
                context,
                selectionSet,
                subscriptionType,
                sourceEvnt,
                path);
        }
    }

    /// <summary>
    ///     Execute subscription source event <paramref name="sourceEvent" /> for given <paramref name="context" />, <paramref name="selectionSet" />,
    ///     <paramref name="subscriptionType" />.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="selectionSet"></param>
    /// <param name="subscriptionType"></param>
    /// <param name="sourceEvent"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static async Task<ExecutionResult> ExecuteSourceEvent(
        QueryContext context,
        SelectionSet selectionSet,
        ObjectDefinition subscriptionType,
        object? sourceEvent,
        NodePath path)
    {
        //todo: use pooled feature or similar
        QueryContext subContext = context with { };
        subContext.Features.Set<IErrorCollectorFeature>(new ConcurrentBagErrorCollectorFeature());

        try
        {
            IReadOnlyDictionary<string, object?> data = await subContext.ExecuteSelectionSet(
                selectionSet,
                subscriptionType,
                sourceEvent,
                path);

            return new ExecutionResult { Data = data, Errors = subContext.GetErrors().ToList() };
        }
        catch (FieldException)
        {
            return new ExecutionResult { Errors = subContext.GetErrors().ToList() };
        }
    }
}