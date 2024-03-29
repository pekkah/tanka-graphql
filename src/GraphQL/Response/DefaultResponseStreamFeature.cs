﻿using Tanka.GraphQL.Features;

namespace Tanka.GraphQL.Response;

public class GraphQLResponseFeature : IResponseStreamFeature
{
    public IAsyncEnumerable<ExecutionResult> Response { get; set; } = AsyncEnumerableEx.Empty<ExecutionResult>();
}