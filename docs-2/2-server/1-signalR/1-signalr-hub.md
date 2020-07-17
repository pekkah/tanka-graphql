## SignalR Hub

Server is implemented as a SignalR Core Hub and it handles queries, mutations
and subscriptions. This projects provides an Apollo Link implementation to be
used with the provided hub.

### GraphQL Server

Configure SignalR server

```csharp
#include::xref://tutorials:GettingStartedServer.cs?s=Tanka.GraphQL.Tutorials.GettingStarted.Startup.AddSignalRServer
```

```csharp
#include::xref://tutorials:GettingStartedServer.cs?s=Tanka.GraphQL.Tutorials.GettingStarted.Startup.UseSignalRServer
```
