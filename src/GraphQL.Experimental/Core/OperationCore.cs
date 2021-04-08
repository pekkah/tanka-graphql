using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Tanka.GraphQL.Experimental.Core
{
    public partial class OperationCore
    {
        private const IReadOnlyDictionary<string, object?>? Null = default(Dictionary<string, object?>); 

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

            var result = Null;
            try
            {
                result = await context.ExecuteSelectionSet(
                    context,
                    query,
                    initialValue,
                    selectionSet,
                    path,
                    cancellationToken);
            }
            catch (FieldException e)
            {
                context.AddError(e);
            }

            yield return new OperationResult
            {
                Data = result,
                Errors = context.Errors
                    .Select(ex => new FieldError(ex.Message, ex.Path, ex.Locations))
                    .ToList()
            };
        }
    }
}