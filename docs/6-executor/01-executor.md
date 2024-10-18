## Executor

GraphQL operations are executor by the GraphQL Executor. The executor is responsible for executing the GraphQL operation and returning the result. The executor is also responsible for handling errors and returning the appropriate error response.

The executor uses `OperationDelegate` to execute the GraphQL operation pipeline. Inputs and outputs to and from the pipeline are contained in the `QueryContext` class which is passed to the delegate by the executor. 

### OperationDelegate

The `OperationDelegate` is a delegate that represents the GraphQL operation pipeline. It is responsible for executing the GraphQL operation and returning the result. The `OperationDelegate` is defined as follows:

```csharp
public delegate Task OperationDelegate(QueryContext context);
```

The `OperationDelegate` takes a `QueryContext` as a parameter and returns a `Task`. The `QueryContext` contains the inputs and outputs to and from the pipeline.

### QueryContext

The `QueryContext` class is used to pass inputs and outputs to and from the GraphQL operation pipeline. It contains the following properties:

- `Request`: The `GraphQLRequest` object that contains the GraphQL query, variables, and other request-related information.
- `RequestCancelled`: A `CancellationToken` that can be used to cancel the request.
- `Features`: A collection of features that can be used to extend the functionality of the `QueryContext`.
- `CoercedVariableValues`: A dictionary that contains the coerced variable values.
- `OperationDefinition`: The `OperationDefinition` object that represents the GraphQL operation to be executed.
- `Schema`: The `ISchema` object that represents the GraphQL schema.
- `Response`: An `IAsyncEnumerable<ExecutionResult>` that represents the response stream.
- `RequestServices`: An `IServiceProvider` that can be used to resolve services.

The `QueryContext` class also contains methods for adding errors, completing values, executing fields, executing selection sets, and getting errors.

### Executor Methods

The `Executor` class has several methods for executing GraphQL operations. The following sections provide detailed explanations and examples of these methods.

#### Execute Method

The `Execute` method is used to execute a GraphQL query or mutation. It takes a `GraphQLRequest` object as a parameter and returns an `ExecutionResult`. The `Execute` method is defined as follows:

```csharp
public async Task<ExecutionResult> Execute(GraphQLRequest request, CancellationToken cancellationToken = default)
{
    QueryContext queryContext = BuildQueryContextAsync(request);
    queryContext.RequestCancelled = cancellationToken;

    IAsyncEnumerable<ExecutionResult> executionResult = ExecuteOperation(queryContext);

    return await executionResult.SingleAsync(queryContext.RequestCancelled);
}
```

The following example shows how to use the `Execute` method to execute a GraphQL query:

```csharp
var schema = new Schema();
var executor = new Executor(schema);

var request = new GraphQLRequest
{
    Query = """
    {
        hello
    }
    """
};

var result = await executor.Execute(request);

Console.WriteLine(result.Data);
```

#### ExecuteQueryOrMutation Method

The `ExecuteQueryOrMutation` method is a static method that is used to execute a query or mutation operation. It takes a `QueryContext` as a parameter and returns a `Task`. The `ExecuteQueryOrMutation` method is defined as follows:

```csharp
public static async Task ExecuteQueryOrMutation(QueryContext context)
{
    var path = new NodePath();
    ObjectDefinition? rootType = context.OperationDefinition.Operation switch
    {
        OperationType.Query => context.Schema.Query,
        OperationType.Mutation => context.Schema.Mutation,
        _ => throw new ArgumentOutOfRangeException()
    };

    if (rootType == null)
        throw new QueryException(
            $"Schema does not support '{context.OperationDefinition.Operation}'. Root type not set.")
        {
            Path = path
        };

    SelectionSet selectionSet = context.OperationDefinition.SelectionSet;

    try
    {
        IReadOnlyDictionary<string, object?> result = await context.ExecuteSelectionSet(
            selectionSet,
            rootType,
            context.Request.InitialValue,
            path);

        context.Response = AsyncEnumerableEx.Return(new ExecutionResult
        {
            Data = result,
            Errors = context.GetErrors().ToList()
        });
        return;
    }
    catch (FieldException e)
    {
        context.AddError(e);
    }

    context.Response = AsyncEnumerableEx.Return(new ExecutionResult
    {
        Data = null,
        Errors = context.GetErrors().ToList()
    });
}
```

The following example shows how to use the `ExecuteQueryOrMutation` method to execute a GraphQL query:

```csharp
var schema = new Schema();
var executor = new Executor(schema);

var request = new GraphQLRequest
{
    Query = """
    {
        hello
    }
    """
};

var queryContext = executor.BuildQueryContextAsync(request);
await Executor.ExecuteQueryOrMutation(queryContext);

var result = await queryContext.Response.SingleAsync();

Console.WriteLine(result.Data);
```

#### ExecuteSubscription Method

The `ExecuteSubscription` method is a static method that is used to execute a subscription operation. It takes a `QueryContext` as a parameter and returns a `Task`. The `ExecuteSubscription` method is defined as follows:

```csharp
public static async Task ExecuteSubscription(QueryContext context)
{
    context.RequestCancelled.ThrowIfCancellationRequested();

    IAsyncEnumerable<object?> sourceStream = await CreateSourceEventStream(
        context,
        context.RequestCancelled);

    IAsyncEnumerable<ExecutionResult> responseStream = MapSourceToResponseEventStream(
        context,
        sourceStream,
        context.RequestCancelled);

    context.Response = responseStream;
}
```

The following example shows how to use the `ExecuteSubscription` method to execute a GraphQL subscription:

```csharp
var schema = new Schema();
var executor = new Executor(schema);

var request = new GraphQLRequest
{
    Query = """
    subscription {
        messageAdded
    }
    """
};

var queryContext = executor.BuildQueryContextAsync(request);
await Executor.ExecuteSubscription(queryContext);

await foreach (var result in queryContext.Response)
{
    Console.WriteLine(result.Data);
}
```

### Pipeline Structure and Flow

The GraphQL execution pipeline is a series of middleware components that process a GraphQL request. Each middleware component in the pipeline has the opportunity to inspect, modify, or short-circuit the request and response. The pipeline is defined using the `OperationDelegateBuilder` class.

#### Defining the Pipeline

The pipeline is defined using the `OperationDelegateBuilder` class. The `OperationDelegateBuilder` class provides methods for adding middleware components to the pipeline. The following example shows how to define a simple pipeline:

```csharp
var builder = new OperationDelegateBuilder();

builder.Use(next => async context =>
{
    // Middleware component 1
    Console.WriteLine("Before Middleware 1");
    await next(context);
    Console.WriteLine("After Middleware 1");
});

builder.Use(next => async context =>
{
    // Middleware component 2
    Console.WriteLine("Before Middleware 2");
    await next(context);
    Console.WriteLine("After Middleware 2");
});

var pipeline = builder.Build();
```

In this example, the pipeline consists of two middleware components. Each middleware component writes a message to the console before and after calling the next middleware component in the pipeline.

#### Using the Pipeline

The pipeline is used to process a GraphQL request. The following example shows how to use the pipeline to process a GraphQL request:

```csharp
var schema = new Schema();
var executor = new Executor(schema);

var request = new GraphQLRequest
{
    Query = """
    {
        hello
    }
    """
};

var queryContext = executor.BuildQueryContextAsync(request);
await pipeline(queryContext);

var result = await queryContext.Response.SingleAsync();

Console.WriteLine(result.Data);
```

In this example, the pipeline is used to process a GraphQL request. The `pipeline` delegate is called with the `queryContext` object, which contains the GraphQL request and other context information. The `queryContext.Response` property contains the response stream, which is an `IAsyncEnumerable<ExecutionResult>`.

#### Pipeline Flowchart

The following flowchart visually represents the pipeline structure and flow:

```plaintext
+-------------------+       +-------------------+       +-------------------+
| Middleware 1      | ----> | Middleware 2      | ----> | Middleware 3      |
|                   |       |                   |       |                   |
| - Before          |       | - Before          |       | - Before          |
| - Next(context)   |       | - Next(context)   |       | - Next(context)   |
| - After           |       | - After           |       | - After           |
+-------------------+       +-------------------+       +-------------------+
```

In this flowchart, each middleware component is represented by a box. The arrows indicate the flow of the request and response through the pipeline. Each middleware component has the opportunity to inspect, modify, or short-circuit the request and response.

[Next](xref://02-simple-usage.md)
