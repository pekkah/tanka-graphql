## Services

Generator supports injecting services into the generated resolvers.

By default the generated code will first check the field arguments for
the argument and then fallback to the DI. It's recommended to use 
[FromServices] attribute to make the intent clear and simplify the generated
code.

### Tanka.GraphQL.Samples.SG.Services

```csharp
#include::xref://samples:GraphQL.Samples.SG.Services/Program.cs
```
