using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Experimental.Definitions
{
    public delegate Task<object?> ExecuteField(
        OperationContext context,
        ObjectDefinition objectDefinition,
        object? objectValue,
        TypeBase fieldType,
        List<FieldSelection> fields,
        NodePath path,
        CancellationToken cancellationToken
    );
}