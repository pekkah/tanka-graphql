## Input Objects

> [Specification](https://facebook.github.io/graphql/June2018/#sec-Input-Objects)

Input objects are created as instances of `InputObjectType`.

### Creating input object

```csharp
#include::xref://tests:graphql.tests/TypeSystem/InputObjectTypeFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.InputObjectTypeFacts.Define
```

> When using List or NonNull wrappers make sure that the wrapped type passes `TypeIs.IsInputType`.

### Input Coercion

```csharp
#include::xref://tests:graphql.tests/TypeSystem/InputObjectTypeFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.InputObjectTypeFacts.Input_coercion
```
