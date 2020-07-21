## Dependency Injection

Tanka GraphQL servers is built on top of the ASP.NET Core and uses
the builtin dependency injection services.

The recommended lifetime of a schema in Tanka is singleton as long
as the schema does not change. This adds a challenge when using
services which require scoped lifetime. Usually the scope is defined
as HTTP request.

### Dependency injection

Tanka GraphQL server starts an dependency injection scope at the beginning of
execution and disposes it at the end. You can access this scope by using `Use<T>`
extensions methods of `IResolverContext`.

Steps

1. Add service using the `Add{Lifetime}` etc. methods of the `IServiceCollection`
2. Use service in resolver using `Use<T>`.

#### 1. Add service

Define service

```csharp
#include::xref://tutorials:GettingStartedServer.cs?s=Tanka.GraphQL.Tutorials.GettingStarted.ResolverController
```

Add service

```csharp
#include::xref://tutorials:GettingStartedServer.cs?s=Tanka.GraphQL.Tutorials.GettingStarted.Startup.AddExecutionScopedService
```

#### 3. Use service in resolver

See [Server](xref://04-server.md) for usage for below method.

```csharp
#include::xref://tutorials:GettingStartedServer.cs?s=Tanka.GraphQL.Tutorials.GettingStarted.SchemaCache.UseService
```
