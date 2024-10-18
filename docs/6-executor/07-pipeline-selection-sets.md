## Pipeline: SelectionSet

The selection sets pipeline is a series of middleware components that process a GraphQL selection set. Each middleware component in the pipeline has the opportunity to inspect, modify, or short-circuit the selection set and response. The pipeline is defined using the `SelectionSetPipelineBuilder` class.

### Defining the Selection Sets Pipeline

The selection sets pipeline is defined using the `SelectionSetPipelineBuilder` class. The `SelectionSetPipelineBuilder` class provides methods for adding middleware components to the pipeline. The following example shows how to define a simple selection sets pipeline:

```csharp
var builder = new SelectionSetPipelineBuilder();

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

var selectionSetPipeline = builder.Build();
```

In this example, the selection sets pipeline consists of two middleware components. Each middleware component writes a message to the console before and after calling the next middleware component in the pipeline.

### Using the Selection Sets Pipeline

The selection sets pipeline is used to process a GraphQL selection set. The following example shows how to use the selection sets pipeline to process a GraphQL selection set:

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
await selectionSetPipeline(queryContext);

var result = await queryContext.Response.SingleAsync();

Console.WriteLine(result.Data);
```

In this example, the selection sets pipeline is used to process a GraphQL selection set. The `selectionSetPipeline` delegate is called with the `queryContext` object, which contains the GraphQL request and other context information. The `queryContext.Response` property contains the response stream, which is an `IAsyncEnumerable<ExecutionResult>`.

[Next](xref://02-simple-usage.md)
