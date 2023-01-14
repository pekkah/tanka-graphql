using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;
using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.Execution;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Tanka.GraphQL.Experimental;

public partial class Executor
{
    private IAsyncEnumerable<ExecutionResult> ExecuteSubscriptionAsync(
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

    private async IAsyncEnumerable<object?> CreateSourceEventStream(
        QueryContext context,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context.Schema.Subscription);

        var subscriptionType = context.Schema.Subscription;
        var groupedFieldSet = SelectionSets.CollectFields(
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

    private async Task<ExecutionResult> ExecuteSourceEvent(
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

            return new ExecutionResult()
            {
                Data = data,
                Errors = subContext.ErrorCollector.GetErrors().Select(e => new ExecutionError()
                {
                    Path = (e as FieldException)?.Path.Segments.ToList() ?? path.Segments.ToList(),
                    Message = e.Message,
                    Extensions = new Dictionary<string, object>()
                    {
                        ["ExceptionType"] = e.GetBaseException().GetType().Name,
                        ["StackTrace"] = e.StackTrace ?? string.Empty
                    }
                }).ToList()
            };
        }
        catch (FieldException x)
        {
            return new ExecutionResult()
            {
                Errors = subContext.ErrorCollector.GetErrors().Select(e => new ExecutionError()
                {
                    Path = (e as FieldException)?.Path.Segments.ToList() ?? path.Segments.ToList(),
                    Message = e.Message,
                    Extensions = new Dictionary<string, object>()
                    {
                        ["ExceptionType"] = e.GetBaseException().GetType().Name,
                        ["StackTrace"] = e.StackTrace ?? string.Empty
                    }
                }).ToList()
            };
        }
    }
}