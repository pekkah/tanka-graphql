## GraphQL WS

Besides the SignalR based server Tanka also provides a graphql-ws protocol
compatible websocket server. This server can be used with
[apollo-link-ws](https://www.apollographql.com/docs/link/links/ws).

### Configure required services

This will add the required services to execution pipeline.

```csharp
#include::xref://tests:graphql.server.tests/Usages/ServerBuilderUsageFacts.cs?s=Tanka.GraphQL.Server.Tests.Usages.ServerBuilderUsageFacts.Configure_WebSockets
```

### Add middleware to app pipeline

```csharp
app.UseWebSockets();
app.UseTankaGraphQLWebSockets("/api/graphql");
```

### Configure protocol

When `connection_init` message is received from client the protocol calls
`AcceptAsync` of the options to accept the connection. By default it accepts
the connection and sends `connection_ack` message back to the client. You can
configure this behavior with your own logic.

```csharp
#include::xref://tests:graphql.server.tests/Usages/ServerBuilderUsageFacts.cs?s=Tanka.GraphQL.Server.Tests.Usages.ServerBuilderUsageFacts.Configure_WebSockets_with_Accept
```
