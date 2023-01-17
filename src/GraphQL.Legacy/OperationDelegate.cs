using System.Collections.Generic;
using System.Threading;
using Tanka.GraphQL.Execution;

namespace Tanka.GraphQL;

public delegate IAsyncEnumerable<ExecutionResult> OperationDelegate(QueryContext context, CancellationToken cancellationToken);
