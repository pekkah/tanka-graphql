## Resolvers

Resolving values is done with two specialized delegates. One is used resolving values and one for subscribing to streams when using subscriptions.

Resolve fields

```csharp
#include::xref://src:graphql/ValueResolution/Resolver.cs
```

Resolve subscription event streams

```csharp
#include::xref://src:graphql/ValueResolution/Subscriber.cs
```

### Resolver

When executing query or mutation `Resolver` is used to resolve the value of the field. Resolver can use the context to access field arguments, schema, and other details about the context of the execution.

### Subscriber

When executing subscription the `Subscriber` is used to resolve the event stream to subscribe into. Both `Subscriber` and `Resolver` are required for field when using subscriptions.

`Subscriber` is responsible for resolving the source stream part of the [Specification](https://facebook.github.io/graphql/June2018/#sec-Source-Stream).

`Resolver` is responsible for resolving the source stream values [Specification](https://facebook.github.io/graphql/June2018/#sec-Response-Stream)

#### Unsubscribe

`Subscriber` is given a cancellation token `unsubscribe` which will change into cancelled state when the request is unsubscribed [Specification](https://facebook.github.io/graphql/June2018/#sec-Unsubscribe).

### Building resolvers with fields

Resolvers can be configured when creating fields. This configuration is used to build the actual resolver when `Schema` is built.

```csharp
#include::xref://tutorials:GettingStarted.cs?s=Tanka.GraphQL.Tutorials.GettingStarted.GettingStarted.Part2_BindResolvers_Manual
```

```csharp
//todo: Subscriber example
```

Resolver middlwares can be used to build an execution chain.

Middlwares are implemented as a delegate method taking in the context and delegate for the next middlware to execute. Last link in the chain is usually the actual resolver but chain can be also interrupted before it by returning a result from the middleware.

Signature of the value resolver middlware:

```csharp
#include::xref://src:graphql/ValueResolution/ResolverMiddleware.cs
```

Signature of the subscription middlware:

```csharp
#include::xref://src:graphql/ValueResolution/SubscriberMiddleware.cs
```

### Using resolver and subscriber maps

Dictionary based implementation is provided for setting up both resolvers and subscribers but other implementations can be easily provided.

```csharp
#include::xref://src:graphql/IResolverMap.cs
```

```csharp
#include::xref://src:graphql/ISubscriberMap.cs
```
