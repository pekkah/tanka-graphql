## Subscriptions

Code generator will generate object definition and extensions methods for adding subscription 
types to the schema. Generator will generate a subscriber for each `IAsyncEnumerable<T>` method
of a class with `[ObjectType]` attribute. You can use provided extension method to add the 
subscription type, subscribers and resolver to the schema.


### Tanka.GraphQL.Samples.SG.Subscription

```csharp
#include::xref://samples:GraphQL.Samples.SG.Subscription/Program.cs
```
