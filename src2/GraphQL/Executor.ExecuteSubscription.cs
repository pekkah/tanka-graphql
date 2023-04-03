using System.Runtime.CompilerServices;
using Tanka.GraphQL.Features;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.SelectionSets;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL;

public partial class Executor
{
    public static Task ExecuteSubscription(QueryContext queryContext)
    {
        queryContext.RequestCancelled.ThrowIfCancellationRequested();

        var sourceStream = CreateSourceEventStream(
            queryContext,
            queryContext.RequestCancelled);

        var responseStream = MapSourceToResponseEventStream(
            queryContext,
            sourceStream,
            queryContext.RequestCancelled);

        queryContext.Response = responseStream;
        return Task.CompletedTask;
    }

    public static async IAsyncEnumerable<object?> CreateSourceEventStream(
        QueryContext context,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context.Schema.Subscription);

        var subscriptionType = context.Schema.Subscription;
        var groupedFieldSet = FieldCollector.CollectFields(
            context.Schema,
            context.Request.Document,
            subscriptionType,
            context.OperationDefinition.SelectionSet,
            context.CoercedVariableValues
        );

        var fields = groupedFieldSet.Values.First();
        var fieldName = fields.First().Name;
        var fieldSelection = fields.First();

        var coercedArgumentValues = ArgumentCoercion.CoerceArgumentValues(
            context.Schema,
            subscriptionType,
            fieldSelection,
            context.CoercedVariableValues);

        var field = context.Schema.GetField(subscriptionType.Name, fieldName);

        if (field is null)
            yield break;

        var path = new NodePath();
        var subscriber = context.Schema.GetSubscriber(subscriptionType.Name, fieldName);

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

        await foreach (var evnt in resolverContext.ResolvedValue.WithCancellation(cancellationToken)) yield return evnt;
    }

    public static async IAsyncEnumerable<ExecutionResult> MapSourceToResponseEventStream(
        QueryContext context,
        IAsyncEnumerable<object?> sourceStream,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context.Schema.Subscription);

        var subscriptionType = context.Schema.Subscription;
        var selectionSet = context.OperationDefinition.SelectionSet;

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

    public static async Task<ExecutionResult> ExecuteSourceEvent(
        QueryContext context,
        SelectionSet selectionSet,
        ObjectDefinition subscriptionType,
        object? sourceEvnt,
        NodePath path)
    {
        var subContext = context with
        {
        };

        subContext.Features.Set<IErrorCollectorFeature>(new ConcurrentBagErrorCollectorFeature());

        try
        {
            var data = await subContext.ExecuteSelectionSet(
                selectionSet,
                subscriptionType,
                sourceEvnt,
                path);

            return new()
            {
                Data = data,
                Errors = subContext.GetErrors().ToList()
            };
        }
        catch (FieldException x)
        {
            return new()
            {
                Errors = subContext.GetErrors().ToList()
            };
        }
    }
}