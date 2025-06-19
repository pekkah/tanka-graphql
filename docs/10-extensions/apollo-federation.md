## Apollo Federation

"Implement a single data graph across multiple services"

> [Specification](https://www.apollographql.com/docs/federation/)

### Scope

Tanka GraphQL implements the service section of the Apollo Federation specification
allowing GraphQL service implemented using it to be joined into graph managed by
[Gateway](https://www.apollographql.com/docs/federation/gateway/).

### Installation

Support is provided as an NuGet package.

```ps
dotnet add package Tanka.GraphQL.Extensions.ApolloFederation
```

### Usage

Creating a service requires two steps:

1. Define [entities](https://www.apollographql.com/docs/federation/entities/) in service schema
2. Create federation service from schema

First step is achived by extending the schema with federation directives which allow you to define your entities.
Extending is done by first adding the directives to schema using `AddFederationDirectives` `SchemaBuilder` extension
method. Use `@key` directive to mark object or interface as an entity and specify key fields.

Second step is after you have your basic schema with entities specified and it will add the required extension
points to schema, mainly additional fields to query root type.

Example

```csharp
#include::xref://tests:GraphQL.Extensions.ApolloFederation.Tests/SchemaFactory.cs?s=Tanka.GraphQL.Extensions.ApolloFederation.Tests.SchemaFactory.Create
```

### Demo

A sample implementation is available in the `dev/GraphQL.Dev.Reviews` project which demonstrates Apollo Federation integration.

To run the sample service:

```ps
cd dev/GraphQL.Dev.Reviews
dotnet run
```

This service provides a federated GraphQL endpoint that can be integrated with other Apollo Federation services using an Apollo Gateway.
