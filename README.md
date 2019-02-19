Tanka GraphQL library
=====================================

[![Build Status](https://dev.azure.com/tanka-ops/graphql/_apis/build/status/graphql)](https://dev.azure.com/tanka-ops/graphql/_build/latest?definitionId=1)


## Features

* Execute queries, mutations and subscriptions
* Validation (Dirty port from graphql-dotnet). Validation is the slowest part of the execution and needs to be redone.
* SignalR hub for streaming queries, mutations and subscriptions
* ApolloLink for the provided SignalR hub


### Feeds

Beta: 
* [Beta docs](https://pekkah.github.io/tanka-graphql/beta/)
* MyGet: https://www.myget.org/F/tanka/api/v3/index.json
* MyGet: https://www.myget.org/F/tanka/npm/

Release
* [Release docs](https://pekkah.github.io/tanka-graphql/)
* From NuGet
* From NPM

[![](https://buildstats.info/nuget/tanka.graphql)](https://www.nuget.org/packages/tanka.graphql/)
[![](https://img.shields.io/npm/v/@tanka/tanka-graphql-server-link.svg?style=popout-square)](https://www.npmjs.com/package/@tanka/tanka-graphql-server-link)


### Install 

```bash
dotnet add tanka.graphql
dotnet add tanka.graphql.server

npm install @tanka/tanka-graphql-server-link
```


## Sample

See [Sample](https://github.com/pekkah/tanka-graphql-samples)


## Develop

### Run benchmarks

```bash
src\graphql.benchmarks> dotnet run --configuration release --framework netcoreapp22
```
