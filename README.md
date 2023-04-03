# Tanka GraphQL library (TGQL)

- Execute queries, mutations and subscriptions
- Validation (new implementation in v0.3.0)
- graphql-ws compatible web socket server (compatible with latest Apollo Client) (since v3)
- Code generation
- New and improved parser for executable and type system documents (since v2.0.0)
- Delegates as resolvers and subscriptions (also middleware) (since v3)
- New middleware based execution pipelines (since v3)
- New server and executor implementations (since v3)
- Apollo Federation subgraph support (since v3)


## Documentation and packages

- [Documentation](https://pekkah.github.io/tanka-graphql/)

Both beta and release packages are available from NuGet and NPM

![Nuget](https://img.shields.io/nuget/v/tanka.graphql?style=flat-square)
![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/tanka.graphql?style=flat-square)


## Samples

See [Samples](https://github.com/pekkah/tanka-graphql/tree/master/samples) in the repository.


## Install

```bash
dotnet package add Tanka.GraphQL
dotnet package add Tanka.GraphQL.Server
```


### Run benchmarks

```bash
src\GraphQL.benchmarks> dotnet run --configuration release --framework net7.0
```
