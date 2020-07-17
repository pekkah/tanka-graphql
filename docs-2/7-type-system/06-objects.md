## Objects

> [Specification](https://facebook.github.io/graphql/June2018/#sec-Objects)

Objects are created as instances of `ObjectType`.

### Creating objects

Objects are created by giving them a unique name and list of fields.

#### With scalar field

Type of field defines the return value of field when resolved during execution.

```csharp
#include::xref://tests:graphql.tests/TypeSystem/ObjectTypeFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.ObjectTypeFacts.With_scalar_field
```

#### With scalar field taking a boolean argument

Fields can take arguments which have a name and type.

```csharp
#include::xref://tests:graphql.tests/TypeSystem/ObjectTypeFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.ObjectTypeFacts.With_scalar_field_with_argument
```
