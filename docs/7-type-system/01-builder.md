## Schema Builder

`SchemaBuilder` is the recommended way of creating `ISchema`s. It provides methods to create types and connect them to each other with fields.

### Creating Root Types

```csharp
#include::xref://tests:graphql.tests/TypeSystem/SchemaBuilderFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.SchemaBuilderFacts.Create_Query
```

```csharp
#include::xref://tests:graphql.tests/TypeSystem/SchemaBuilderFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.SchemaBuilderFacts.Create_Mutation
```

```csharp
#include::xref://tests:graphql.tests/TypeSystem/SchemaBuilderFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.SchemaBuilderFacts.Create_Subscription
```

### Creating types

```csharp
#include::xref://tests:graphql.tests/TypeSystem/SchemaBuilderFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.SchemaBuilderFacts.Create_Object
```

```csharp
#include::xref://tests:graphql.tests/TypeSystem/SchemaBuilderFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.SchemaBuilderFacts.Create_Interface
```

```csharp
#include::xref://tests:graphql.tests/TypeSystem/SchemaBuilderFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.SchemaBuilderFacts.Create_Union
```

```csharp
#include::xref://tests:graphql.tests/TypeSystem/SchemaBuilderFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.SchemaBuilderFacts.Create_Enum
```

```csharp
#include::xref://tests:graphql.tests/TypeSystem/SchemaBuilderFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.SchemaBuilderFacts.Create_Scalar
```

```csharp
#include::xref://tests:graphql.tests/TypeSystem/SchemaBuilderFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.SchemaBuilderFacts.Create_Scalar_without_converter
```

```csharp
#include::xref://tests:graphql.tests/TypeSystem/SchemaBuilderFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.SchemaBuilderFacts.Create_InputObject
```

```csharp
#include::xref://tests:graphql.tests/TypeSystem/SchemaBuilderFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.SchemaBuilderFacts.Create_DirectiveType
```

### Build and validate schema

Normal (will throw ValidationException if schema is not valid)

```csharp
#include::xref://tests:graphql.tests/TypeSystem/SchemaBuilderFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.SchemaBuilderFacts.Build
```

Without throwing validation exception

```csharp
#include::xref://tests:graphql.tests/TypeSystem/SchemaBuilderFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.SchemaBuilderFacts.Build_and_validate_schema
```

### Connecting types using fields

```csharp
#include::xref://tests:graphql.tests/TypeSystem/SchemaBuilderFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.SchemaBuilderFacts.Create_Object_field
```

```csharp
#include::xref://tests:graphql.tests/TypeSystem/SchemaBuilderFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.SchemaBuilderFacts.Create_Interface_field
```

```csharp
#include::xref://tests:graphql.tests/TypeSystem/SchemaBuilderFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.SchemaBuilderFacts.Create_InputObject_field
```

### Configuring new schema based on existing schema

```csharp
#include::xref://tests:graphql.tests/TypeSystem/SchemaBuilderFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.SchemaBuilderFacts.Use_existing_schema
```

### Merging schemas

```csharp
#include::xref://tests:graphql.tests/TypeSystem/SchemaBuilderFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.SchemaBuilderFacts.Merge_schemas
```

### Making executable schema

```csharp
#include::xref://tests:graphql.tests/TypeSystem/SchemaBuilderFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.SchemaBuilderFacts.Make_executable_schema
```

### Special cases

```csharp
#include::xref://tests:graphql.tests/TypeSystem/SchemaBuilderFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.SchemaBuilderFacts.Build_with_circular_reference_between_two_objects
```
