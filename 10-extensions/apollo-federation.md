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

Demo is provided as an fork of the [federation demo](https://github.com/pekkah/federation-demo). It's modified to use
[Tanka.GraphQL.Dev.Reviews](https://github.com/pekkah/tanka-graphql/tree/master/dev/GraphQL.Dev.Reviews) project as an replacement for reviews service.

```ps
git clone https://github.com/pekkah/tanka-graphql.git

cd tanka-graphql/dev/GraphQL.Dev.Reviews
dotnet run
```

```ps
git clone https://github.com/pekkah/federation-demo.git
cd federation-demo
git switch tanka-graphql-reviews-service
npm run start-services

# in a yet another prompt
npm run start-gateway

# open http://localhost:4000 in your browser to test the service
```
