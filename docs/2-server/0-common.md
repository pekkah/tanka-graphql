## Common options 

Tanka provides SignalR hub and websockets server. Both of these use
same underlying services for query execution.


### Configure schema

Use `ISchema` from services. `GetSchema` returns a `ValueTask<ISchema>`
for async initialization. It's recommended not to return new schema 
for every call as `GetSchema` is called for every execution.

```csharp
// configure options to use schema from services
services.AddTankaSchemaOptions()
        .Configure<ISchema>((options, schema) =>
        {
            options.GetSchema = query => new ValueTask<ISchema>(schema);
        });
```