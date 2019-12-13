Tanka GraphQL library (TGQL)
=====================================

[![Build Status](https://dev.azure.com/tanka-ops/graphql/_apis/build/status/graphql?branchName=master)](https://dev.azure.com/tanka-ops/graphql/_build/latest?definitionId=1&branchName=master)


## Features

* Execute queries, mutations and subscriptions
* Validation (new implementation in v0.3.0)
* SignalR hub for streaming queries, mutations and subscriptions
* ApolloLink for the provided SignalR hub
* Apollo GraphQL WebSockets (apollo-link-ws) compatible web socket server (since v0.8.0)


### Feeds

Beta 

* [Documentation](https://pekkah.github.io/tanka-graphql/beta/)

Release

* [Documentation](https://pekkah.github.io/tanka-graphql/)

Both beta and release packages are available from NuGet and NPM

![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/tanka.graphql?style=flat-square)
![Nuget](https://img.shields.io/nuget/v/tanka.graphql?style=flat-square)
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

### Run the dev harness

This repo includes a sample application which is used for testing
and development of the SignalR client and the server.

Open the `tanka-graphql.sln` and start the `graphql.dev.chat.web` project. This will start a simple chat server using the tanka.graphql.server.

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
