## Resolvers

Resolving values is done with two specialized delegates. One is used resolving values and one for subscribing to streams when using subscriptions.

Resolve fields
[{tanka.graphql.resolvers.Resolver}]

Resolve subscription event streams
[{tanka.graphql.resolvers.Subscriber}]


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

[{tanka.graphql.tests.type.SchemaBuilderFacts.Create_Field_Resolver}]

[{tanka.graphql.tests.type.SchemaBuilderFacts.Create_Field_Subscriber}]

Resolver middlwares can be used to build an execution chain. 

Middlwares are implemented as a delegate method taking in the context and delegate for the next middlware to execute. Last link in the chain is usually the actual resolver but chain can be also interrupted before it by returning a result from the middleware.

Signature of the value resolver middlware:
[{tanka.graphql.resolvers.ResolverMiddleware}]

Signature of the subscription middlware:
[{tanka.graphql.resolvers.SubscriberMiddleware}]



### Using resolver and subscriber maps

In some cases it's useful to be able to build the resolvers separately from the schema building. For that purpose `SchemaTools` provide a method to bind resolvers to fields by using `IResolverMap` and `ISubscriberMap`.

[{tanka.graphql.tests.type.SchemaBuilderFacts.Make_executable_schema}]

Dictionary based implementation is provided for setting up both resolvers and subscribers but other implementations can be easily provided. 

[{tanka.graphql.IResolverMap}]
[{tanka.graphql.ISubscriberMap}]

