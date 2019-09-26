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

[{Tanka.GraphQL.TypeSystem.INamedType}]

### Wrapping types

Wrapping types implement the `IWrappingType`

[{Tanka.GraphQL.TypeSystem.IWrappingType}]

Two built-in implementations are provided
- `List`
- `NonNull`


### Input and output types

Is input type?

[{Tanka.GraphQL.Tests.Execution.TypeIsFacts.IsInputType}]

[{Tanka.GraphQL.Tests.Execution.TypeIsFacts.ValidInputTypes}]


Is output type?

[{Tanka.GraphQL.Tests.Execution.TypeIsFacts.IsOutputType}]

[{Tanka.GraphQL.Tests.Execution.TypeIsFacts.ValidOutputTypes}]


