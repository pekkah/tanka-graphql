## Descriptions

> [Specification](https://facebook.github.io/graphql/June2018/#sec-Descriptions)

> NotSupported: the parser used does not currently support block string
> documentation. 
> Issue [#24](https://github.com/pekkah/tanka-graphql/issues/24)

`Meta` class provides the fields for descriptions. Following elements can be described using the `Meta` class. Usually by providing it as an constructor parameter.

- `Argument`
- `DirectiveType`
- `EnumType` and values
- `Field`
- `InputObjectField`
- `InputObjectType`
- `InterfaceType`
- `ObjectType`
- `ScalarType`
- `UnionType`


### Example: Field description

Description for the field is given by providing instance of `Meta`.

[{tanka.graphql.tests.descriptions.FieldFacts.Describe}]

If no meta is given then the default `Meta` is used with empty value as description.

[{tanka.graphql.tests.descriptions.FieldFacts.Meta_is_always_available}]



