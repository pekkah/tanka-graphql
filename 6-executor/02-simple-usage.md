## Simple Usage

`Executor` provides a simple way to execute GraphQL queries. The following example shows how to execute a simple query that returns a scalar value. This uses the instance method `Executor.Execute` with [default pipeline](xref://05-pipeline.md).

```csharp
#include::xref://tests:GraphQL.Tests/Executor.QueryFacts.cs?s=Tanka.GraphQL.Tests.QueryFacts.Simple_Scalar
```

The `Executor` instance is created with `Schema`. The `Schema` is used to resolve the query and the `Query` is the query document to execute passed into the `Execute` method as GraphQL request document. GraphQLRequest object contains also possible operation name and variables.


[Next](xref://03-queries-and-mutations.md)