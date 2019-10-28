## Dependency Injection

Tanka GraphQL servers is built on top of the ASP.NET Core and uses
the builtin dependency injection services. 

The recommended lifetime of a schema in Tanka is singleton as long
as the schema does not change. This adds a challenge when using
services which require scoped lifetime. Usually the scope is defined
as HTTP request.


### Dependency injection using context extensions

Tanka GraphQL allows extending the execution trough extensions. Server
adds an context extension which carries object trough the execution pipeline. 
These objects can be access during the value resolution. As the objects are
resolved from `IServiceProvider` during the beginning of GraphQL request execution
they will have their lifetime specified when they're added to the `IServiceCollection`.

Steps required to register context extension using `IServiceCollection`:
1. Define context object and add to services
3. Use service in resolver


#### 1. Define context and add to services

Define context object
[{Tanka.GraphQL.Tutorials.GettingStarted.ResolverController}]

Add context to services and add context extension
[{Tanka.GraphQL.Tutorials.GettingStarted.Startup.AddContextExtension}]


#### 3. Use service in resolver 

See [Server](0-getting-started/04-server.html) for usage for below method.

[{Tanka.GraphQL.Tutorials.GettingStarted.SchemaCache.UseContextExtension}]