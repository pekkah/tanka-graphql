## Dependency Injection

Tanka GraphQL servers is built on top of the ASP.NET Core and uses
the builtin dependency injection services.

The recommended lifetime of a schema in Tanka is singleton as long
as the schema does not change. This adds a challenge when using
services which require scoped lifetime. Usually the scope is defined
as HTTP request.

todo: update