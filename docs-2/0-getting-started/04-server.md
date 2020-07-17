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
#include::xref://tutorials:GettingStartedServer.cs?s=Tanka.GraphQL.Tutorials.GettingStarted.SchemaCache.Create
```

### 2. Configure

Next needed step is to configure server options. These options tell
the executor where to get the schema and also allows configuring the validation
rules for the execution. By default all validation rules from the GraphQL
specification are included.

```csharp
#include::xref://tutorials:GettingStartedServer.cs?s=Tanka.GraphQL.Tutorials.GettingStarted.Startup.AddTanka
```

### 3. Configure server

Server components require calling their `Add*` methods to add required
services and configure their options. Depending on the server also
middleware needs to be configured using the paired `Use*` methods.

Configure SignalR server

```csharp
#include::xref://tutorials:GettingStartedServer.cs?s=Tanka.GraphQL.Tutorials.GettingStarted.Startup.AddSignalRServer
```

Use SignalR server

```csharp
#include::xref://tutorials:GettingStartedServer.cs?s=Tanka.GraphQL.Tutorials.GettingStarted.Startup.UseSignalRServer
```

Configure GraphQL WS server

```csharp
#include::xref://tutorials:GettingStartedServer.cs?s=Tanka.GraphQL.Tutorials.GettingStarted.Startup.AddWebSocketsServer
```

Use GraphQL WS Server

```csharp
#include::xref://tutorials:GettingStartedServer.cs?s=Tanka.GraphQL.Tutorials.GettingStarted.Startup.UseWebSocketsServer
```

### 4. HTTP API

Tanka Server does not provide out of the box "server" for basic
HTTP/JSON API as that can be easily implemented.

Here's an example of ASP.NET Core MVC Controller taken from the
`Tanka.GraphQL.Samples.Chat.Web` project included in the solution.

`IQueryStreamService` is registered by the `AddTankaGraphQL`
and is also used by the SignalR and GraphQL-WS based servers to
execute the queries.

[{Tanka.GraphQL.Samples.Chat.Web.Controllers.QueryController}]

```csharp
#include::xref://dev:graphql.dev.chat.web\Controllers\QueryController.cs?s=Tanka.GraphQL.Samples.Chat.Web.Controllers.QueryController
```
