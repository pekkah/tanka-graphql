## Scalars

> [Specification](https://facebook.github.io/graphql/June2018/#sec-Scalars)

Scalars are created as instances of `ScalarType`.

### Built-in

These are provided as static properties on `ScalarType`.

- `ScalarType.Boolean`
- `ScalarType.Int`
- `ScalarType.ID`
- `ScalarType.Float`
- `ScalarType.String`

In addition following non-null instances are provided for convenience.

- `ScalarType.NonNullBoolean`
- `ScalarType.NonNullInt`
- `ScalarType.NonNullID`
- `ScalarType.NonNullFloat`
- `ScalarType.NonNullString`

Also standard collection is provided

```csharp
#include::xref://src:graphql/TypeSystem/ScalarType.cs?s=Tanka.GraphQL.TypeSystem.ScalarType.Standard
```

### Custom scalars

Create instance of `ScalarType` and provide name and metadata. Value converter
is also needed when building schema with custom scalar in it.

#### Example:

Scalar:

```csharp
#include::xref://src:graphql/TypeSystem/ScalarType.cs?s=Tanka.GraphQL.TypeSystem.ScalarType.ID
```

Converter:

```csharp
#include::xref://src:graphql/TypeSystem/ValueSerialization/IdConverter.cs
```
