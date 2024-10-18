## Queries and Mutations

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
