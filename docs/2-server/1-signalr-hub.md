## Server

Server is implemented as a SignalR Core Hub and it handles queries, mutations
and subscriptions. This projects provides an Apollo Link implementation to be
used with the provided hub.

### GraphQL Query Streaming Hub

```csharp
// Configure Services
services.AddSignalR()
    // add GraphQL query streaming hub
    .AddQueryStreamHub();

// Configure App
app.UseSignalR(routes =>
{
    routes.MapHub<QueryStreamHub>(new PathString("/graphql"));
});

```