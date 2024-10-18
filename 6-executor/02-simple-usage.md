## Simple Usage

`Executor` provides a simple way to execute GraphQL queries. The following example shows how to execute a simple query that returns a scalar value. This uses the instance method `Executor.Execute` with [default pipeline](xref://05-pipeline.md).

```csharp
#include::xref://tests:GraphQL.Tests/Executor.QueryFacts.cs?s=Tanka.GraphQL.Tests.QueryFacts.Simple_Scalar
```

The `Executor` instance is created with `Schema`. The `Schema` is used to resolve the query and the `Query` is the query document to execute passed into the `Execute` method as GraphQL request document. GraphQLRequest object contains also possible operation name and variables.

### Detailed Explanation of `Executor.Execute` Method

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

### Example of Using `Executor.Execute` Method with Variables

The following example shows how to use the `Execute` method to execute a GraphQL query with variables:

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
                            Name = new Name("hello"),
                            Arguments = new Arguments
                            {
                                new Argument
                                {
                                    Name = new Name("name"),
                                    Value = new VariableReference
                                    {
                                        Name = new Name("name")
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    },
    Variables = new Dictionary<string, object?>
    {
        { "name", "World" }
    }
};

var result = await executor.Execute(request);

Console.WriteLine(result.Data);
```

### Example of Using `Executor.Execute` Method with Operation Name

The following example shows how to use the `Execute` method to execute a GraphQL query with an operation name:

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
                Name = new Name("GetHello"),
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
            },
            new OperationDefinition
            {
                Operation = OperationType.Query,
                Name = new Name("GetGoodbye"),
                SelectionSet = new SelectionSet
                {
                    Selections = new List<ISelection>
                    {
                        new FieldSelection
                        {
                            Name = new Name("goodbye")
                        }
                    }
                }
            }
        }
    },
    OperationName = "GetHello"
};

var result = await executor.Execute(request);

Console.WriteLine(result.Data);
```

### Complex Example: Combining Multiple Middleware Components

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

[Next](xref://03-queries-and-mutations.md)