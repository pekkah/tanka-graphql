## Scalars

> [Specification](https://facebook.github.io/graphql/June2018/#sec-Scalars)

Scalars are created as instances of `ScalarDefinition`.

### Built-in

- `Boolean`
- `Int`
- `ID`
- `Float`
- `String`

Built-in scalars are automatically included in the schema. See [Builder](xref://01_1-builder.md) or
[ExecutableSchemaBuilder](xref://01_2-executablebuilder.md) for more information on how to replace
or remove them.

For example of how to add a custom scalar, see [Custom Scalars](xref://start:06-custom-scalars.md).