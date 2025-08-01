using System;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic.FileIO;

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
    public static async Task ExecuteSubscription(QueryContext context)
    {
        context.RequestCancelled.ThrowIfCancellationRequested();

        IAsyncEnumerable<object?> sourceStream = await CreateSourceEventStream(
            context,
            context.RequestCancelled);

        IAsyncEnumerable<ExecutionResult> responseStream = MapSourceToResponseEventStream(
            context,
            sourceStream,
            context.RequestCancelled);

        context.Response = responseStream;
    }


    /// <summary>
    ///     Create subscription source event stream for given <paramref name="context" />.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="QueryException"></exception>
    public static async Task<IAsyncEnumerable<object?>> CreateSourceEventStream(
        QueryContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context.Schema.Subscription);

        ObjectDefinition? subscriptionType = context.Schema.Subscription;
        var fieldCollector = context.RequestServices.GetRequiredService<IFieldCollector>();
        IReadOnlyDictionary<string, List<FieldSelection>> groupedFieldSet = fieldCollector.CollectFields(
            context.Schema,
            context.Request.Query,
            subscriptionType,
            context.OperationDefinition.SelectionSet,
            context.CoercedVariableValues
        );

        List<FieldSelection> fields = groupedFieldSet.Values.First();
        FieldSelection fieldSelection = fields.First();
        Name fieldName = fieldSelection.Name;

        IReadOnlyDictionary<string, object?> coercedArgumentValues = ArgumentCoercion.CoerceArgumentValues(
            context.Schema,
            subscriptionType,
            fieldSelection,
            context.CoercedVariableValues);

        FieldDefinition? field = context.Schema.GetField(subscriptionType.Name, fieldName);

        if (field is null)
            return AsyncEnumerableEx.Empty<object?>();

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

        try
        {
            await subscriber(resolverContext, cancellationToken);

            if (resolverContext.ResolvedValue is null)
                return AsyncEnumerableEx.Empty<object?>();
        }
        catch (Exception exception)
        {
            if (exception is not FieldException)
                throw new FieldException(exception.Message, exception)
                {
                    ObjectDefinition = subscriptionType,
                    Field = field,
                    Selection = fieldSelection,
                    Path = path
                };

            throw;
        }

        return Core(resolverContext, cancellationToken);

        static async IAsyncEnumerable<object?> Core(
            SubscriberContext resolverContext,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await using var e = resolverContext.ResolvedValue!
                .GetAsyncEnumerator(cancellationToken);

            while (true)
            {
                try
                {
                    if (!await e.MoveNextAsync())
                    {
                        yield break;
                    }

                }
                catch (Exception exception)
                {
                    if (exception is not FieldException)
                        throw new FieldException(exception.Message, exception)
                        {
                            ObjectDefinition = resolverContext.ObjectDefinition,
                            Field = resolverContext.Field,
                            Selection = resolverContext.Selection,
                            Path = resolverContext.Path
                        };

                    throw;
                }

                yield return e.Current;
            }
        }
    }

    /// <summary>
    ///     Map subscription source event stream to response event stream for given <paramref name="context" />
    ///     and <paramref name="sourceStream" />.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="sourceStream"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static IAsyncEnumerable<ExecutionResult> MapSourceToResponseEventStream(
        QueryContext context,
        IAsyncEnumerable<object?> sourceStream,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context.Schema.Subscription);

        ObjectDefinition? subscriptionType = context.Schema.Subscription;
        SelectionSet selectionSet = context.OperationDefinition.SelectionSet;

        return Core(context, sourceStream, subscriptionType, selectionSet, cancellationToken);

        static async IAsyncEnumerable<ExecutionResult> Core(
            QueryContext context,
            IAsyncEnumerable<object?> sourceStream,
            ObjectDefinition subscriptionType,
            SelectionSet selectionSet,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {

            await foreach (var sourceEvnt in sourceStream.WithCancellation(cancellationToken))
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
        catch (FieldException x)
        {
            subContext.AddError(x);
            return new ExecutionResult { Errors = subContext.GetErrors().ToList() };
        }
    }
}