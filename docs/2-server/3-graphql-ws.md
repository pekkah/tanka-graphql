## GraphQL WS

Besides the SignalR based server Tanka also provides a graphql-ws protocol 
compatible websocket server. This server can be used with 
[apollo-link-ws](https://www.apollographql.com/docs/link/links/ws).

### Add required services

This will add the required services and Apollo Tracing extension to execution 
pipeline.

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

### Configure protocol

When `connection_init` message is received from client the protocol calls
`AcceptAsync` of the options to accept the connection. By default it accepts
the connection and sends `connection_ack` message back to the client. You can 
configure this behavior with your own logic.

```csharp
services.AddTankaWebSocketServerWithTracing()
        .Configure<IAuthenticationService>(
            (options, authentication) => options.AcceptAsync = async context =>
            {
                var token = context.Message.Payload.SelectToken("authToken");
                
                // true when accepted; otherwise false
                var isValid = await authentication.IsValidAsync(token);
                
                if (!isValid) 
                {
                    // you must decide what kind of message to send back to the client
                    // in case the connection is not accepted.
                    await context.Output.WriteAsync(new OperationMessage
                    {
                        Type = MessageType.GQL_CONNECTION_ERROR,
                        Id = context.Message.Id
                    });
                    
                    // complete the output forcing the server to disconnect
                    context.Output.Complete();
                }
                else 
                {
                    await context.Output.WriteAsync(new OperationMessage
                    {
                        Type = MessageType.GQL_CONNECTION_ACK
                    });
                }
            });
```


