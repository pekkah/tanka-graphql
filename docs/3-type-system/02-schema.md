## Schema

> [Specification](https://facebook.github.io/graphql/June2018/#sec-Schema)

Schema is implemented as class `GraphSchema` and it implements the `ISchema` interface.

[{tanka.graphql.type.ISchema}]

> Rarely accessing the actual implemention class `GraphSchema` is required

Schema is immutable and cannot be changed directly once created. To create `Schema` instance you need to provide `query` root object at minimum. Optionally `mutation` and `subscription` roots can be also provided.


### Basics

Built by `SchemaBuilder`

[{tanka.graphql.tests.type.SchemaFacts.SchemaFacts}]

Query root is `ObjectType`

[{tanka.graphql.tests.type.SchemaFacts.Roots_Query}]

Mutation is optional

[{tanka.graphql.tests.type.SchemaFacts.Roots_Mutation}]

Subscription is optional

[{tanka.graphql.tests.type.SchemaFacts.Roots_Subscription}]


### Querying types

Query named types
[{tanka.graphql.tests.type.SchemaFacts.GetNamedType}]
[{tanka.graphql.tests.type.SchemaFacts.QueryNamedTypes}]


Query directives
[{tanka.graphql.tests.type.SchemaFacts.GetDirective}]
[{tanka.graphql.tests.type.SchemaFacts.QueryDirectives}]


Query fields
[{tanka.graphql.tests.type.SchemaFacts.GetField}]
[{tanka.graphql.tests.type.SchemaFacts.GetFields}]


Query input fields
[{tanka.graphql.tests.type.SchemaFacts.GetInputField}]
[{tanka.graphql.tests.type.SchemaFacts.GetInputFields}]


#### Defaults

By default Skip and Include directives are included
[{tanka.graphql.tests.type.SchemaFacts.Included_directives}]

By default all standard `ScalarType`s are included
[{tanka.graphql.tests.type.SchemaFacts.Included_scalars}]





