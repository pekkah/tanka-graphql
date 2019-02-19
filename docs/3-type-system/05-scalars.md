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

[{tanka.graphql.type.ScalarType.Standard}]


### Custom scalars

Create instance of `ScalarType` and provide name, value converter and metadata.

#### Example:

Scalar:
[{tanka.graphql.type.ScalarType.ID}]

Converter:
[{tanka.graphql.type.converters.IdConverter}]