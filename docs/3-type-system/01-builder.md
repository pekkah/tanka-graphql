## Schema Builder

`SchemaBuilder` is the recommended way of creating `ISchema`s. It provides methods to create types and connect them to each other with fields.


### Creating Root Types

[{tanka.graphql.tests.type.SchemaBuilderFacts.Create_Query}]

[{tanka.graphql.tests.type.SchemaBuilderFacts.Create_Mutation}]

[{tanka.graphql.tests.type.SchemaBuilderFacts.Create_Subscription}]


### Creating types

[{tanka.graphql.tests.type.SchemaBuilderFacts.Create_Object}]

[{tanka.graphql.tests.type.SchemaBuilderFacts.Create_Interface}]

[{tanka.graphql.tests.type.SchemaBuilderFacts.Create_Union}]

[{tanka.graphql.tests.type.SchemaBuilderFacts.Create_Enum}]

[{tanka.graphql.tests.type.SchemaBuilderFacts.Create_Scalar}]

[{tanka.graphql.tests.type.SchemaBuilderFacts.Create_InputObject}]

[{tanka.graphql.tests.type.SchemaBuilderFacts.Create_DirectiveType}]


### Build and validate schema

> NotImplemented: future enhancement

[{tanka.graphql.tests.type.SchemaBuilderFacts.Build_and_validate_schema}]


### Connecting types using fields

[{tanka.graphql.tests.type.SchemaBuilderFacts.Create_Object_field}]

[{tanka.graphql.tests.type.SchemaBuilderFacts.Create_Interface_field}]

[{tanka.graphql.tests.type.SchemaBuilderFacts.Create_InputObject_field}]


### Configuring new schema based on existing schema

[{tanka.graphql.tests.type.SchemaBuilderFacts.Use_existing_schema}]


### Merging schemas

[{tanka.graphql.tests.type.SchemaBuilderFacts.Merge_schemas}]


### Making executable schema

[{tanka.graphql.tests.type.SchemaBuilderFacts.Make_executable_schema}]


### Special cases

[{tanka.graphql.tests.type.SchemaBuilderFacts.Build_with_circular_reference_between_two_objects}]