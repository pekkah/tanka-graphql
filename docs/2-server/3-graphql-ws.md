## GraphQL WS

Besides the SignalR based server Tanka also provides a graphql-ws protocol compatible websocket server. This server can be used with [apollo-link-ws](https://www.apollographql.com/docs/link/links/ws).

### Add required services

This will add the required services and Apollo Tracing extension to execution pipeline.

```csharp
services.AddTankaWebSocketServerWithTracing();

// or without the tracing extension
services.AddTankaWebSocketServer();

// you might also need to configure the websockets
services.AddWebSockets(options =>
{
    options.AllowedOrigins.Add("https://localhost:5000");
});
```

### Add middleware to app pipeline

```csharp
// websockets server
app.UseWebSockets();
app.UseTankaWebSocketServer(new WebSocketServerOptions()
{
    Path = "/api/graphql"
});
```

