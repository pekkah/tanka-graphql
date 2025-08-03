using System.Runtime.CompilerServices;

using Tanka.GraphQL.Features;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL;

public partial class Executor
{
    /// <summary>
    ///     Static method to execute a query or mutation operation using the given <paramref name="context"/>.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="QueryException"></exception>
    public static async Task ExecuteQueryOrMutation(QueryContext context)
    {
        var path = new NodePath();
        ObjectDefinition? rootType = context.OperationDefinition.Operation switch
        {
            OperationType.Query => context.Schema.Query,
            OperationType.Mutation => context.Schema.Mutation,
            _ => throw new ArgumentOutOfRangeException()
        };

        if (rootType == null)
            throw new QueryException(
                $"Schema does not support '{context.OperationDefinition.Operation}'. Root type not set.")
            {
                Path = path
            };

        SelectionSet selectionSet = context.OperationDefinition.SelectionSet;

        try
        {
            IReadOnlyDictionary<string, object?> result = await context.ExecuteSelectionSet(
                selectionSet,
                rootType,
                context.Request.InitialValue,
                path);

            // Check if we have incremental delivery
            var incrementalFeature = context.Features.Get<IIncrementalDeliveryFeature>();
            if (incrementalFeature?.HasIncrementalWork == true)
            {
                // Set up streaming response
                context.Response = CreateIncrementalResponse(result, context, incrementalFeature);
                return;
            }

            // No incremental delivery, return single result
            context.Response = AsyncEnumerableEx.Return(new ExecutionResult
            {
                Data = result,
                Errors = context.GetErrors().ToList()
            });
            return;
        }
        catch (FieldException e)
        {
            context.AddError(e);
        }

        context.Response = AsyncEnumerableEx.Return(new ExecutionResult
        {
            Data = null,
            Errors = context.GetErrors().ToList()
        });
    }

    private static async IAsyncEnumerable<ExecutionResult> CreateIncrementalResponse(
        IReadOnlyDictionary<string, object?> initialData,
        QueryContext context,
        IIncrementalDeliveryFeature incrementalFeature,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var initialErrors = context.GetErrors().ToList();

        // Complete the incremental feature to signal no more deferred work will be registered
        incrementalFeature.Complete();

        // Yield the initial result
        yield return new ExecutionResult
        {
            Data = initialData,
            Errors = initialErrors.Any() ? initialErrors : null,
            HasNext = true
        };

        // Stream the deferred results
        await foreach (var incrementalPayload in incrementalFeature.GetDeferredResults(cancellationToken))
        {
            yield return new ExecutionResult
            {
                Incremental = new[] { incrementalPayload },
                HasNext = false // For now, assume each incremental payload is the last one
            };
        }
    }
}