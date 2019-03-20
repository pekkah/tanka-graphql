Tanka GraphQL library
=====================================

[![Build Status](https://dev.azure.com/tanka-ops/graphql/_apis/build/status/graphql?branchName=master)](https://dev.azure.com/tanka-ops/graphql/_build/latest?definitionId=1&branchName=master)


## Features

* Execute queries, mutations and subscriptions
* Validation (new implementation in v0.3.0)
* SignalR hub for streaming queries, mutations and subscriptions
* ApolloLink for the provided SignalR hub


### Feeds

Beta 

[![](https://img.shields.io/myget/tanka/vpre/tanka.graphql.svg?style=popout-square)](https://www.myget.org/feed/tanka/package/nuget/tanka.graphql)
[![](https://img.shields.io/npm/v/@tanka/tanka-graphql-server-link/latest.svg?registry_uri=https%3A%2F%2Fwww.myget.org%2FF%2Ftanka%2Fnpm%2F&style=popout-square)](https://www.myget.org/feed/tanka/package/npm/@tanka/tanka-graphql-server-link)

* [Documentation](https://pekkah.github.io/tanka-graphql/beta/)
* MyGet: https://www.myget.org/F/tanka/api/v3/index.json
* MyGet: https://www.myget.org/F/tanka/npm/


Release

[![](https://buildstats.info/nuget/tanka.graphql)](https://www.nuget.org/packages/tanka.graphql/)
[![](https://img.shields.io/npm/v/@tanka/tanka-graphql-server-link.svg?style=popout-square)](https://www.npmjs.com/package/@tanka/tanka-graphql-server-link)

* [Documentation](https://pekkah.github.io/tanka-graphql/)
* From NuGet
* From NPM


### Install 

```bash
dotnet add tanka.graphql
dotnet add tanka.graphql.server

npm install @tanka/tanka-graphql-server-link
```


## Sample

See [Sample](https://github.com/pekkah/tanka-graphql-samples)


## Develop

### Run the dev harness

This repo includes a sample application which is used for testing
and development of the SignalR client and the server.

Open the `tanka-graphql.sln` and start the `graphql.samples.chat.web` project. This will start a simple chat server using the tanka.graphql.server.

Start the client by following instructions below:

```bash
# Install dependencies
src\graphql.server.link> yarn install

# Link the server-link
src\graphql.server.link> yarn link

# Watch for source changes and recompile the link
src\graphql.server.link> yarn watch

# Install dependencies
dev\graphql.dev.chat.ui> yarn install

# Use the linked server-link
dev\graphql.dev.chat.ui> yarn link @tanka/tanka-graphql-server-link

# Watch for source changes and recompile the sample
dev\graphql.dev.chat.ui> yarn start
```


### Run benchmarks

```bash
src\graphql.benchmarks> dotnet run --configuration release --framework netcoreapp22
```
