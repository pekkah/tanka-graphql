## Bind resolvers

Resolvers are used to resolve the data for the query, or execute mutations. They bind the execution of
GraphQL operation into your logic.

> See also
>
> - [Specification](https://graphql.github.io/graphql-spec/June2018/#sec-Value-Resolution)


### Bind using `SchemaBuilder`

When building the executable schema using `Build` the `SchemaBuilder` takes an `IResolverMap` and/or `ISubscriberMap` values. These
maps include the resolvers for fields of object types. 

+ Simplest form of a resolver is an delegate returning a value.

```csharp
#include::xref://tutorials:GettingStarted.cs?s=Tanka.GraphQL.Tutorials.GettingStarted.GettingStarted.Part2_BindResolvers_ReturnValue
```


+ Use `ResolverContext`: context provides information about the query being executed.

```csharp
#include::xref://tutorials:GettingStarted.cs?s=Tanka.GraphQL.Tutorials.GettingStarted.GettingStarted.Part2_BindResolvers_UseContext
```


+ You can access properties of the `ResolverContext` by named parameters keeping the resolver simple. In this case we access
the value of the parent called `objectValue`.

```csharp
#include::xref://tutorials:GettingStarted.cs?s=Tanka.GraphQL.Tutorials.GettingStarted.GettingStarted.Part2_BindResolvers_ObjectValue
```

+ For complex scenarios usage of `ResolversBuilder` is recommended

```csharp
#include::xref://tutorials:GettingStarted.cs?s=Tanka.GraphQL.Tutorials.GettingStarted.GettingStarted.Part2_BindResolvers_ResolversBuilder
```