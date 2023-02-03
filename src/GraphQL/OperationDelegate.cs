namespace Tanka.GraphQL;

public delegate Task OperationDelegate(QueryContext context, CancellationToken cancellationToken);