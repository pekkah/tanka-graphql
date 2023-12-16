## Executor

GraphQL operations are executor by the GraphQL Executor. The executor is responsible for executing the GraphQL operation and returning the result. The executor is also responsible for handling errors and returning the appropriate error response.

The executor uses `OperationDelegate` to execute the GraphQL operation pipeline. Inputs and outputs to and from the pipeline are contained in the `QueryContext` class which is passed to the delegate by the executor. 

```csharp
#include::xref://src:GraphQL/OperationDelegate.cs
```

[Next](xref://02-simple-usage.md)