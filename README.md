# Tanka GraphQL library

- Execute queries, mutations and subscriptions
- Validation 
- Source Generator based source code generation (Schema types from classes)
- New and improved parser for executable and type system documents
- Delegates as resolvers and subscriptions (also middleware)
- New middleware based execution pipelines (since v3)
- New server and executor implementations (since v3)
- Apollo Federation subgraph support (since v3)
- [graphql-ws](https://github.com/enisdenjo/graphql-ws) compatible web socket server (compatible with latest Apollo Client) (since v3)
- @oneOf directive support for polymorphic input types (Stage 3 RFC)


## Documentation and packages

- [Documentation](https://pekkah.github.io/tanka-graphql/)

Both beta and release packages are available from NuGet and NPM

![Nuget](https://img.shields.io/nuget/v/tanka.graphql?style=flat-square)
![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/tanka.graphql?style=flat-square)


## Samples

See [Samples](https://github.com/pekkah/tanka-graphql/tree/master/samples) in the repository.
See [Tanka Chat](https://github.com/pekkah/tanka-graphql-samples) for a more complete example.


## Install

```bash
dotnet package add Tanka.GraphQL
dotnet package add Tanka.GraphQL.Server
```


### Run benchmarks

```bash
src\GraphQL.benchmarks> dotnet run --configuration release --framework net8.0
```
