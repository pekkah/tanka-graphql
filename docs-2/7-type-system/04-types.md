## Types

> [Specification](https://facebook.github.io/graphql/June2018/#sec-Types)

Following types are supported:

- `ScalarType`
- `ObjectType`
- `InterfaceType`
- `UnionType`
- `EnumType`
- `InputObjectType`

These all exists in `Tanka.GraphQL.type` namespace and implement `INamedType` interface

```csharp
#include::xref://src:graphql/TypeSystem/INamedType.cs?s=Tanka.GraphQL.TypeSystem.INamedType
```

### Wrapping types

Wrapping types implement the `IWrappingType`

```csharp
#include::xref://src:graphql/TypeSystem/IWrappingType.cs?s=Tanka.GraphQL.TypeSystem.IWrappingType
```

Two built-in implementations are provided

- `List`
- `NonNull`

### Input and output types

Is input type?

```csharp
#include::xref://tests:graphql.tests/Execution/TypeIsFacts.cs?s=Tanka.GraphQL.Tests.Execution.TypeIsFacts.IsInputType
```

```csharp
#include::xref://tests:graphql.tests/Execution/TypeIsFacts.cs?s=Tanka.GraphQL.Tests.Execution.TypeIsFacts.ValidInputTypes
```

Is output type?

```csharp
#include::xref://tests:graphql.tests/Execution/TypeIsFacts.cs?s=Tanka.GraphQL.Tests.Execution.TypeIsFacts.IsOutputType
```

```csharp
#include::xref://tests:graphql.tests/Execution/TypeIsFacts.cs?s=Tanka.GraphQL.Tests.Execution.TypeIsFacts.ValidOutputTypes
```
