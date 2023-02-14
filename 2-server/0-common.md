## Common options

Tanka provides SignalR hub and websockets server. Both of these use
same underlying services for query execution.

### Add required common services

Add services required for executing GraphQL queries, mutations
and subscriptions. Main service added is `IQueryStreamService`
which handles the plumping of execution.

```csharp
todo: add sample
```

### Configure schema

Configure `ISchema` for execution by providing a factory function (can be async)
used to get the schema for execution.

Simple without dependencies

```csharp
todo: add sample
```

Overloads are provided for providing a function with dependencies resolved from
services.

```csharp
todo: add sample
```

### Configure rules

Configure validation rules for execution. Note that by default all rules
specified in the specification are included.

Add MaxCost validation rule

```csharp
todo: add sample
```

Remove all rules

```csharp
todo: add sample
```

With up to three dependencies resolved from service provider

```csharp
todo: add sample
```

### Add extensions

Add Apollo tracing extension

```csharp
todo: add sample
```
