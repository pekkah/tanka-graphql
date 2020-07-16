## Schema Introspection

> [Specification](https://facebook.github.io/graphql/June2018/#sec-Schema-Introspection)

Introspecting a schema produces an executable schema. Queries can be executed against this schema normally. 

Create and introspect a schema
[{Tanka.GraphQL.Tests.Introspection.IntrospectSchemaFacts.IntrospectSchemaFacts}]

Combination of the source schema and the introspection schema can be used to provide the normal executable schema with introspection support.

[{Tanka.GraphQL.Tests.Data.Starwars.StarwarsFixture.CreateSchema}]

### Type Kinds

Scalar
[{Tanka.GraphQL.Tests.Introspection.IntrospectSchemaFacts.Type_ScalarType}]

Object type
[{Tanka.GraphQL.Tests.Introspection.IntrospectSchemaFacts.Type_ObjectType}]

Union type
[{Tanka.GraphQL.Tests.Introspection.IntrospectSchemaFacts.Type_UnionType}]

Interface type
[{Tanka.GraphQL.Tests.Introspection.IntrospectSchemaFacts.Type_InterfaceType}]

Enum type
[{Tanka.GraphQL.Tests.Introspection.IntrospectSchemaFacts.Type_EnumType}]

Input object
[{Tanka.GraphQL.Tests.Introspection.IntrospectSchemaFacts.Type_InputObjectType}]





