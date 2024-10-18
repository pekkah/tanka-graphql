## Pipeline

The GraphQL execution pipeline is a series of middleware components that process a GraphQL request. Each middleware component in the pipeline has the opportunity to inspect, modify, or short-circuit the request and response. The pipeline is defined using the `OperationDelegateBuilder` class.

### Defining the Pipeline

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

### Using the Pipeline

The pipeline is used to process a GraphQL request. The following example shows how to use the pipeline to process a GraphQL request:

```csharp
var schema = new Schema();
var executor = new Executor(schema);

var request = new GraphQLRequest
{
    Query = new ExecutableDocument
    {
        Definitions = new List<IDefinition>
        {
            new OperationDefinition
            {
                Operation = OperationType.Query,
                SelectionSet = new SelectionSet
                {
                    Selections = new List<ISelection>
                    {
                        new FieldSelection
                        {
                            Name = new Name("hello")
                        }
                    }
                }
            }
        }
    }
};

var queryContext = executor.BuildQueryContextAsync(request);
await pipeline(queryContext);

var result = await queryContext.Response.SingleAsync();

Console.WriteLine(result.Data);
```

In this example, the pipeline is used to process a GraphQL request. The `pipeline` delegate is called with the `queryContext` object, which contains the GraphQL request and other context information. The `queryContext.Response` property contains the response stream, which is an `IAsyncEnumerable<ExecutionResult>`.

### Pipeline Flowchart

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

### Detailed Example: Custom Middleware Components

The following example demonstrates advanced usage and customization of the pipelines by combining multiple middleware components:

```csharp
var builder = new OperationDelegateBuilder();

builder.Use(next => async context =>
{
    // Middleware component 1: Logging
    Console.WriteLine("Before Middleware 1");
    await next(context);
    Console.WriteLine("After Middleware 1");
});

builder.Use(next => async context =>
{
    // Middleware component 2: Authentication
    if (!context.Request.Headers.ContainsKey("Authorization"))
    {
        throw new UnauthorizedAccessException("Authorization header is missing.");
    }
    await next(context);
});

builder.Use(next => async context =>
{
    // Middleware component 3: Custom logic
    Console.WriteLine("Executing custom logic");
    await next(context);
});

var pipeline = builder.Build();

var schema = new Schema();
var executor = new Executor(schema);

var request = new GraphQLRequest
{
    Query = new ExecutableDocument
    {
        Definitions = new List<IDefinition>
        {
            new OperationDefinition
            {
                Operation = OperationType.Query,
                SelectionSet = new SelectionSet
                {
                    Selections = new List<ISelection>
                    {
                        new FieldSelection
                        {
                            Name = new Name("hello")
                        }
                    }
                }
            }
        }
    }
};

var queryContext = executor.BuildQueryContextAsync(request);
await pipeline(queryContext);

var result = await queryContext.Response.SingleAsync();

Console.WriteLine(result.Data);
```

In this example, the pipeline consists of three middleware components: logging, authentication, and custom logic. Each middleware component has the opportunity to inspect, modify, or short-circuit the request and response.

[Next](xref://02-simple-usage.md)
