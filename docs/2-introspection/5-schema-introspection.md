## Schema Introspection

> [Specification](https://facebook.github.io/graphql/June2018/#sec-Schema-Introspection)

Introspecting a schema produces an executable schema. Queries can be executed against this schema normally. 

Create and introspect a schema
[{tanka.graphql.tests.introspection.IntrospectSchemaFacts.IntrospectSchemaFacts}]

Combination of the source schema and the introspection schema can be used to provide the normal executable schema with introspection support.

[{tanka.graphql.tests.data.starwars.StarwarsFixture.MakeExecutableAsync}]

### Type Kinds

Scalar
[{tanka.graphql.tests.introspection.IntrospectSchemaFacts.Type_ScalarType}]

Object type
[{tanka.graphql.tests.introspection.IntrospectSchemaFacts.Type_ObjectType}]

Union type
[{tanka.graphql.tests.introspection.IntrospectSchemaFacts.Type_UnionType}]

Interface type
[{tanka.graphql.tests.introspection.IntrospectSchemaFacts.Type_InterfaceType}]

Enum type
[{tanka.graphql.tests.introspection.IntrospectSchemaFacts.Type_EnumType}]

Input object
[{tanka.graphql.tests.introspection.IntrospectSchemaFacts.Type_InputObjectType}]





