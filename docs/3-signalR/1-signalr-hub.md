## SignalR Hub

Server is implemented as a SignalR Core Hub and it handles queries, mutations
and subscriptions. This projects provides an Apollo Link implementation to be
used with the provided hub.

### GraphQL Server Hub

```csharp
// Configure Services
services.AddSignalR(options => options.EnableDetailedErrors = true)
    // add GraphQL query streaming hub
    .AddTankaServerHubWithTracing();

// Configure App
app.UseSignalR(routes => routes.MapTankaServerHub("/graphql"));

// or with options
app.UseSignalR(routes => routes.MapTankaServerHub("/graphql", options => 
{
    // configure signalr hub options
}));
```