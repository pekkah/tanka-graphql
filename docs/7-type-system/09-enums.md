## Enums

> [Specification](https://facebook.github.io/graphql/June2018/#sec-Enums)

Enums are created as instances of `EnumType`.

### Creating enum

Direction enum

```csharp
#include::xref://tests:graphql.tests/TypeSystem/EnumTypeFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.EnumTypeFacts.Define_enum
```

### Serialization

From input value

```csharp
#include::xref://tests:graphql.tests/TypeSystem/EnumTypeFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.EnumTypeFacts.ParseValue
```

From AST node value

```csharp
#include::xref://tests:graphql.tests/TypeSystem/EnumTypeFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.EnumTypeFacts.ParseLiteral
```

Serialize

```csharp
#include::xref://tests:graphql.tests/TypeSystem/EnumTypeFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.EnumTypeFacts.Serialize
```
