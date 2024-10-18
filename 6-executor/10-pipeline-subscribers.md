## Pipeline: Subscriber

The subscribers pipeline is a series of middleware components that process a GraphQL subscriber. Each middleware component in the pipeline has the opportunity to inspect, modify, or short-circuit the subscriber and response. The pipeline is defined using the `SubscriberPipelineBuilder` class.

### Defining the Subscribers Pipeline

The subscribers pipeline is defined using the `SubscriberPipelineBuilder` class. The `SubscriberPipelineBuilder` class provides methods for adding middleware components to the pipeline. The following example shows how to define a simple subscribers pipeline:

```csharp
var builder = new SubscriberPipelineBuilder();

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

var subscribersPipeline = builder.Build();
```

In this example, the subscribers pipeline consists of two middleware components. Each middleware component writes a message to the console before and after calling the next middleware component in the pipeline.

### Using the Subscribers Pipeline

The subscribers pipeline is used to process a GraphQL subscriber. The following example shows how to use the subscribers pipeline to process a GraphQL subscriber:

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
                Operation = OperationType.Subscription,
                SelectionSet = new SelectionSet
                {
                    Selections = new List<ISelection>
                    {
                        new FieldSelection
                        {
                            Name = new Name("messageAdded")
                        }
                    }
                }
            }
        }
    }
};

var queryContext = executor.BuildQueryContextAsync(request);
await subscribersPipeline(queryContext);

await foreach (var result in queryContext.Response)
{
    Console.WriteLine(result.Data);
}
```

In this example, the subscribers pipeline is used to process a GraphQL subscriber. The `subscribersPipeline` delegate is called with the `queryContext` object, which contains the GraphQL request and other context information. The `queryContext.Response` property contains the response stream, which is an `IAsyncEnumerable<ExecutionResult>`.

[Next](xref://02-simple-usage.md)
