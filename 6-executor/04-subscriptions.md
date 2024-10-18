## Subscriptions

### Detailed Explanation of `Executor.ExecuteSubscription` Method

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

### Example of Using `Executor.ExecuteSubscription` Method

The following example shows how to use the `ExecuteSubscription` method to execute a GraphQL subscription:

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
await Executor.ExecuteSubscription(queryContext);

await foreach (var result in queryContext.Response)
{
    Console.WriteLine(result.Data);
}
```

[Next](xref://02-simple-usage.md)
