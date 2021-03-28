using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Experimental
{
    public partial class OperationCore
    {
        public static async Task<object?> ExecuteField(
            OperationContext context,
            ObjectDefinition objectDefinition,
            object? objectValue,
            TypeBase fieldType,
            IReadOnlyList<FieldSelection> fields,
            NodePath path,
            CoerceArgumentValues coerceArgumentValues,
            ResolveFieldValue resolveFieldValue,
            CompleteValue completeValue,
            CancellationToken cancellationToken)
        {
            var field = fields.First();
            var fieldName = field.Name;

            var argumentValues = await coerceArgumentValues(
                context.Schema,
                objectDefinition,
                field,
                cancellationToken);

            var resolvedValue = await resolveFieldValue(
                context,
                objectDefinition,
                objectValue,
                fieldName,
                argumentValues,
                path,
                cancellationToken);

            return await completeValue(
                context,
                fieldType,
                fields,
                resolvedValue,
                path,
                cancellationToken);
        }
    }
}