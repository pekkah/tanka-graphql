using System.Collections.Generic;

namespace Tanka.GraphQL.Server;

public delegate IAsyncEnumerable<ExecutionResult> GraphQLRequestDelegate(GraphQLRequestContext context);