## Types

> [Specification](https://facebook.github.io/graphql/June2018/#sec-Types)

Following types are supported:
- `ScalarType`
- `ObjectType`
- `InterfaceType`
- `UnionType`
- `EnumType`
- `InputObjectType`

These all exists in `tanka.graphql.type` namespace and implement `INamedType` interface


### Wrapping types

Wrapping types implement the `IWrappingType`

[{tanka.graphql.type.IWrappingType}]

Two built-in implementations are provided
- `List`
- `NonNull`


### Input and output types

Is input type?

[{tanka.graphql.tests.execution.TypeIsFacts.IsInputType}]

[{tanka.graphql.tests.execution.TypeIsFacts.ValidInputTypes}]


Is output type?

[{tanka.graphql.tests.execution.TypeIsFacts.IsOutputType}]

[{tanka.graphql.tests.execution.TypeIsFacts.ValidOutputTypes}]


