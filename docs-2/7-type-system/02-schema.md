## Schema

> [Specification](https://facebook.github.io/graphql/June2018/#sec-Schema)

Schema is defined by `ISchema`-interface.

```csharp
#include::xref://src:graphql/TypeSystem/ISchema.cs
```

> Rarely accessing the actual implemention class `GraphSchema` is required

Schema is immutable and cannot be changed directly once created. To create `Schema` instance you need to provide `query` root object at minimum. Optionally `mutation` and `subscription` roots can be also provided.

### Basics

Built by `SchemaBuilder`

```csharp
#include::xref://tests:graphql.tests/TypeSystem/SchemaFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.SchemaFacts.SchemaFacts
```

Query root is `ObjectType`

```csharp
#include::xref://tests:graphql.tests/TypeSystem/SchemaFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.SchemaFacts.Roots_Query
```

Mutation is optional

```csharp
#include::xref://tests:graphql.tests/TypeSystem/SchemaFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.SchemaFacts.Roots_Mutation
```

Subscription is optional

```csharp
#include::xref://tests:graphql.tests/TypeSystem/SchemaFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.SchemaFacts.Roots_Subscription
```

### Querying types

Query named types

```csharp
#include::xref://tests:graphql.tests/TypeSystem/SchemaFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.SchemaFacts.GetNamedType
#include::xref://tests:graphql.tests/TypeSystem/SchemaFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.SchemaFacts.QueryNamedTypes
```

Query directives

```csharp
#include::xref://tests:graphql.tests/TypeSystem/SchemaFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.SchemaFacts.GetDirective
#include::xref://tests:graphql.tests/TypeSystem/SchemaFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.SchemaFacts.QueryDirectives
```

Query fields

```csharp
#include::xref://tests:graphql.tests/TypeSystem/SchemaFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.SchemaFacts.GetField
#include::xref://tests:graphql.tests/TypeSystem/SchemaFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.SchemaFacts.GetFields
```

Query input fields

```csharp
#include::xref://tests:graphql.tests/TypeSystem/SchemaFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.SchemaFacts.GetInputField
#include::xref://tests:graphql.tests/TypeSystem/SchemaFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.SchemaFacts.GetInputFields
```

#### Defaults

By default Skip and Include directives are included

```csharp
#include::xref://tests:graphql.tests/TypeSystem/SchemaFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.SchemaFacts.Included_directives
```

By default all standard `ScalarType`s are included

```csharp
#include::xref://tests:graphql.tests/TypeSystem/SchemaFacts.cs?s=Tanka.GraphQL.Tests.TypeSystem.SchemaFacts.Included_scalars
```
