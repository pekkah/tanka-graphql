using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Tanka.GraphQL.Experimental
{
    public partial class OperationCore
    {
        public static async IAsyncEnumerable<OperationResult> ExecuteOperation(
            OperationContext context,
            object? initialValue = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();

            var path = new NodePath();
            var query = context.Schema.Query;
            var selectionSet = context.Operation.SelectionSet;

            var result = await context.ExecuteSelectionSet(
                context,
                query,
                initialValue,
                selectionSet,
                path,
                cancellationToken);

            yield return new OperationResult
            {
                Data = result.Data,
                Errors = result.Errors
            };
        }
    }
}