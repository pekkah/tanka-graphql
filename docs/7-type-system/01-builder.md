## Schema Builder

`SchemaBuilder` is the recommended way of creating `ISchema`s. It provides methods to create types and connect them to each other with fields.


### Creating Root Types

[{Tanka.GraphQL.Tests.TypeSystem.SchemaBuilderFacts.Create_Query}]

[{Tanka.GraphQL.Tests.TypeSystem.SchemaBuilderFacts.Create_Mutation}]

[{Tanka.GraphQL.Tests.TypeSystem.SchemaBuilderFacts.Create_Subscription}]


### Creating types

[{Tanka.GraphQL.Tests.TypeSystem.SchemaBuilderFacts.Create_Object}]

[{Tanka.GraphQL.Tests.TypeSystem.SchemaBuilderFacts.Create_Interface}]

[{Tanka.GraphQL.Tests.TypeSystem.SchemaBuilderFacts.Create_Union}]

[{Tanka.GraphQL.Tests.TypeSystem.SchemaBuilderFacts.Create_Enum}]

[{Tanka.GraphQL.Tests.TypeSystem.SchemaBuilderFacts.Create_Scalar}]

[{Tanka.GraphQL.Tests.TypeSystem.SchemaBuilderFacts.Create_InputObject}]

[{Tanka.GraphQL.Tests.TypeSystem.SchemaBuilderFacts.Create_DirectiveType}]


### Build and validate schema

> NotImplemented: future enhancement

[{Tanka.GraphQL.Tests.TypeSystem.SchemaBuilderFacts.Build_and_validate_schema}]


### Connecting types using fields

[{Tanka.GraphQL.Tests.TypeSystem.SchemaBuilderFacts.Create_Object_field}]

[{Tanka.GraphQL.Tests.TypeSystem.SchemaBuilderFacts.Create_Interface_field}]

[{Tanka.GraphQL.Tests.TypeSystem.SchemaBuilderFacts.Create_InputObject_field}]


### Configuring new schema based on existing schema

[{Tanka.GraphQL.Tests.TypeSystem.SchemaBuilderFacts.Use_existing_schema}]


### Merging schemas

[{Tanka.GraphQL.Tests.TypeSystem.SchemaBuilderFacts.Merge_schemas}]


### Making executable schema

[{Tanka.GraphQL.Tests.TypeSystem.SchemaBuilderFacts.Make_executable_schema}]


### Special cases

[{Tanka.GraphQL.Tests.TypeSystem.SchemaBuilderFacts.Build_with_circular_reference_between_two_objects}]