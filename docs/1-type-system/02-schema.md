## Schema

> [Specification](https://facebook.github.io/graphql/June2018/#sec-Schema)

Schema is implemented as class `Schema` and it implements the `ISchema` interface.

[{tanka.graphql.type.ISchema}]

Schema is immutable and cannot be changed directly once initialized. To create `Schema` instance you need to provide `query` root object at minimum. Optionally `mutation` and `subscription` roots can be also provided.

> Currently you must initialize `Schema` by using `InitializeAsync`. This
> requirement will be removed in later versions. Also immutability will 
> be fixed.

### Root Object Types

Query is required

[{tanka.graphql.tests.type.SchemaFacts.Set_Query}]

[{tanka.graphql.tests.type.SchemaFacts.Require_Query}]


Mutation is optional

[{tanka.graphql.tests.type.SchemaFacts.Set_Mutation}]


Subscription is optional

[{tanka.graphql.tests.type.SchemaFacts.Set_Subscription}]


#### Default types

By default Skip and Include directives are included

[{tanka.graphql.tests.type.SchemaFacts.Initialize_directives_with_skip_and_include}]


#### Including types

Types are included in the `Schema` if they're referenced by root graph types. 

[{tanka.graphql.tests.type.SchemaFacts.Initialize_types}]

[{tanka.graphql.tests.type.SchemaFacts.Initialize_types_with_found_scalars}]

Type is only included once even if referenced multiple times.

[{tanka.graphql.tests.type.SchemaFacts.Initialize_types_no_duplicates}]


#### Types referenced by name only

If type is referenced as `NamedTypeReference` the actual type must be included. When schema is initialized the named type reference is replaced with the actual type.

[{tanka.graphql.tests.type.SchemaFacts.Initialize_heal_schema}]





