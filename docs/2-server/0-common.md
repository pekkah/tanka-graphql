## Common options 

Tanka provides SignalR hub and websockets server. Both of these use
same underlying services for query execution.

### Configure execution options

Use `ISchema` from services

```csharp
// configure options to use schema from
services.AddTankaExecutionOptions()
        .Configure<ISchema>((options, schema) =>
        {
            options.Schema = schema;
        });
```