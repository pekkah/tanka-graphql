## Dependency Injection

Tanka GraphQL servers is built on top of the ASP.NET Core and uses
the builtin dependency injection services.

The recommended lifetime of a schema in Tanka is singleton as long
as the schema does not change. This adds a challenge when using
services which require scoped lifetime. Usually the scope is defined
as HTTP request.

### Lets define a dependency

```csharp
#include::xref://tutorials:GettingStarted.cs?s=Tanka.GraphQL.Tutorials.GettingStarted.GettingStarted.Service
```


+ Use `ResolverContext`: context has an property called `RequestServices.

```csharp
#include::xref://tutorials:GettingStarted.cs?s=Tanka.GraphQL.Tutorials.GettingStarted.GettingStarted.Part5_ServiceProvider_RequestServices
```

+ Use a parameter on delegate, if the property with the name does not exists on `ResolverContext` it will be resolved from `RequestServices`

```csharp
#include::xref://tutorials:GettingStarted.cs?s=Tanka.GraphQL.Tutorials.GettingStarted.GettingStarted.Part5_ServiceProvider_Delegate_with_parameters
```