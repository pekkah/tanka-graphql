namespace Tanka.GraphQL;

/// <summary>
///     Operation delegate for executing GraphQL operations using the <paramref name="context"/>.
/// </summary>
/// <param name="context"></param>
/// <returns></returns>
public delegate Task OperationDelegate(QueryContext context);