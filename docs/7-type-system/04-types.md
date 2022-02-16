## Types

> [Specification](https://facebook.github.io/graphql/June2018/#sec-Types)

Following types are supported:

- `ScalarType`
- `ObjectType`
- `InterfaceType`
- `UnionType`
- `EnumType`
- `InputObjectType`

These all exists in `Tanka.GraphQL.Language.Nodes` namespace and implement `TypeDefinition` base class

```csharp
// todo: docs
```

### Wrapping types

Two built-in implementations are provided

- `LisType`
- `NonNullType`