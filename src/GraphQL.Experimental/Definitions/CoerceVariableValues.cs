using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Experimental.Definitions
{
    public delegate ValueTask<IReadOnlyDictionary<string, object?>> CoerceVariableValues(
        ExecutableSchema schema,
        OperationDefinition operation,
        IReadOnlyDictionary<string, object?>? variableValues,
        CancellationToken cancellationToken
    );
}