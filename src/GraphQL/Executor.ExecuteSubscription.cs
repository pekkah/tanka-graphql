using System.Runtime.CompilerServices;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.SelectionSets;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL;

public partial class Executor
{
    private IAsyncEnumerable<ExecutionResult> ExecuteSubscription(
        QueryContext queryContext,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sourceStream = CreateSourceEventStream(
            queryContext,
            cancellationToken);

        var responseStream = MapSourceToResponseEventStream(
            queryContext,
            sourceStream,
            cancellationToken);

        return responseStream;
    }

    public async IAsyncEnumerable<object?> CreateSourceEventStream(
        QueryContext context,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context.Schema.Subscription);

        var subscriptionType = context.Schema.Subscription;
        var groupedFieldSet = SelectionSetExtensions.CollectFields(
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
            Arguments = coercedArgumentValues,
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

    public async IAsyncEnumerable<ExecutionResult> MapSourceToResponseEventStream(
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

    public async Task<ExecutionResult> ExecuteSourceEvent(
        QueryContext context,
        SelectionSet selectionSet,
        ObjectDefinition subscriptionType,
        object? sourceEvnt,
        NodePath path)
    {
        var subContext = context with
        {
            ErrorCollector = new ConcurrentBagErrorCollector()
        };

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
                Errors = subContext.ErrorCollector.GetErrors().ToList()
            };
        }
        catch (FieldException x)
        {
            return new()
            {
                Errors = subContext.ErrorCollector.GetErrors().ToList()
            };
        }
    }
}