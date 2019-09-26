## Schema

> [Specification](https://facebook.github.io/graphql/June2018/#sec-Schema)

Schema is implemented as class `GraphSchema` and it implements the `ISchema` interface.

[{Tanka.GraphQL.TypeSystem.ISchema}]

> Rarely accessing the actual implemention class `GraphSchema` is required

Schema is immutable and cannot be changed directly once created. To create `Schema` instance you need to provide `query` root object at minimum. Optionally `mutation` and `subscription` roots can be also provided.


### Basics

Built by `SchemaBuilder`

[{Tanka.GraphQL.Tests.TypeSystem.SchemaFacts.SchemaFacts}]

Query root is `ObjectType`

[{Tanka.GraphQL.Tests.TypeSystem.SchemaFacts.Roots_Query}]

Mutation is optional

[{Tanka.GraphQL.Tests.TypeSystem.SchemaFacts.Roots_Mutation}]

Subscription is optional

[{Tanka.GraphQL.Tests.TypeSystem.SchemaFacts.Roots_Subscription}]


### Querying types

Query named types
[{Tanka.GraphQL.Tests.TypeSystem.SchemaFacts.GetNamedType}]
[{Tanka.GraphQL.Tests.TypeSystem.SchemaFacts.QueryNamedTypes}]


Query directives
[{Tanka.GraphQL.Tests.TypeSystem.SchemaFacts.GetDirective}]
[{Tanka.GraphQL.Tests.TypeSystem.SchemaFacts.QueryDirectives}]


Query fields
[{Tanka.GraphQL.Tests.TypeSystem.SchemaFacts.GetField}]
[{Tanka.GraphQL.Tests.TypeSystem.SchemaFacts.GetFields}]


Query input fields
[{Tanka.GraphQL.Tests.TypeSystem.SchemaFacts.GetInputField}]
[{Tanka.GraphQL.Tests.TypeSystem.SchemaFacts.GetInputFields}]


#### Defaults

By default Skip and Include directives are included
[{Tanka.GraphQL.Tests.TypeSystem.SchemaFacts.Included_directives}]

By default all standard `ScalarType`s are included
[{Tanka.GraphQL.Tests.TypeSystem.SchemaFacts.Included_scalars}]





