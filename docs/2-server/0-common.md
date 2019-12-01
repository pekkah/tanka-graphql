## Common options 

Tanka provides SignalR hub and websockets server. Both of these use
same underlying services for query execution.

### Add required common services

Add services required for executing GraphQL queries, mutations
and subscriptions. Main service added is `IQueryStreamService`
which handles the plumping of execution.

[{Tanka.GraphQL.Server.Tests.Usages.ServerBuilderUsageFacts.AddTankaGraphQL}]


### Configure schema

Configure `ISchema` for execution by providing a factory function (can be async)
used to get the schema for execution.

Simple without dependencies

[{Tanka.GraphQL.Server.Tests.Usages.ServerBuilderUsageFacts.Configure_Schema}]

Overloads are provided for providing a function with dependencies resolved from
services.

[{Tanka.GraphQL.Server.Tests.Usages.ServerBuilderUsageFacts.Configure_Schema_with_dependency}]


### Configure rules

Configure validation rules for execution. Note that by default all rules
specified in the specification are included. 

Add MaxCost validation rule

[{Tanka.GraphQL.Server.Tests.Usages.ServerBuilderUsageFacts.Configure_Rules}]

Remove all rules

[{Tanka.GraphQL.Server.Tests.Usages.ServerBuilderUsageFacts.Configure_Rules_remove_all}]

With up to three dependencies resolved from service provider

[{Tanka.GraphQL.Server.Tests.Usages.ServerBuilderUsageFacts.Configure_Rules_with_dependency}]


### Add extensions

Add Apollo tracing extension

[{Tanka.GraphQL.Server.Tests.Usages.ServerBuilderUsageFacts.Add_Extension}]

