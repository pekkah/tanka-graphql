## Common options 

Tanka provides SignalR hub and websockets server. Both of these use
same underlying services for query execution.

### Add required common services

Add services required for executing GraphQL queries, mutations
and subscriptions.

```csharp
services.AddTankaGraphQL();
```


### Add schema

Configure `ISchema` for execution.

```csharp
services.AddTankaGraphQL()
    .WithSchema<SchemaCache>(async cache => await cache.GetOrAdd());
```


### Add rules

Configure validation rules for execution. Note that by default all rules
specified in the specification are included.

```csharp
services.AddTankaGraphQL()
        .WithRules(rules => rules.Concat(new[]
        {
            CostAnalyzer.MaxCost(100, 1, true)
        }).ToArray());
```


### Add extensions

Add tracing extension

```csharp
services.AddTankaGraphQL()
                .WithExtension<TraceExtension>();
```

