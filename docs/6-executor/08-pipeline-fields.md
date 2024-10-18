## Pipeline: Field

The fields pipeline is a series of middleware components that process a GraphQL field. Each middleware component in the pipeline has the opportunity to inspect, modify, or short-circuit the field and response. The pipeline is defined using the `FieldDelegateBuilder` class.

### Defining the Fields Pipeline

The fields pipeline is defined using the `FieldDelegateBuilder` class. The `FieldDelegateBuilder` class provides methods for adding middleware components to the pipeline. The following example shows how to define a simple fields pipeline:

```csharp
var builder = new FieldDelegateBuilder();

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

var fieldsPipeline = builder.Build();
```

In this example, the fields pipeline consists of two middleware components. Each middleware component writes a message to the console before and after calling the next middleware component in the pipeline.

### Using the Fields Pipeline

The fields pipeline is used to process a GraphQL field. The following example shows how to use the fields pipeline to process a GraphQL field:

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
await fieldsPipeline(queryContext);

var result = await queryContext.Response.SingleAsync();

Console.WriteLine(result.Data);
```

In this example, the fields pipeline is used to process a GraphQL field. The `fieldsPipeline` delegate is called with the `queryContext` object, which contains the GraphQL request and other context information. The `queryContext.Response` property contains the response stream, which is an `IAsyncEnumerable<ExecutionResult>`.

[Next](xref://02-simple-usage.md)
