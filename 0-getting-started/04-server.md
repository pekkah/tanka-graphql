## Server

Tanka provides server components in `Tanka.GraphQL.server` NuGet
package. It can be used to get up and running or if more control is
required then you can implement your own server.

> See also
>
> - [Server](xref://server:0-common.md)

Tanka Server provides two server components which can be used together
or separately.

- SignalR - server built on top of SignalR Core
- GraphQL-WS - custom WebSockets server supporting `graphql-ws` messages

Following steps are required to get the server running with your schema.

1. Build schema
2. Configure schema options
3. Configure server
4. HTTP API

### 1. Build schema

First step for either server is to configure your schema options
for execution. Schema should be built only once and cached as singleton.

```csharp
//todo: update sample
```

### 2. Configure

Next needed step is to configure server options. These options tell
the executor where to get the schema and also allows configuring the validation
rules for the execution. By default all validation rules from the GraphQL
specification are included.

```csharp
//todo: update sample
```

### 3. Configure server

Server components require calling their `Add*` methods to add required
services and configure their options. Depending on the server also
middleware needs to be configured using the paired `Use*` methods.

Configure SignalR server

```csharp
//todo: update sample
```

Use SignalR server

```csharp
//todo: update sample
```

Configure GraphQL WS server

```csharp
//todo: update sample
```

Use GraphQL WS Server

```csharp
//todo: update sample
```