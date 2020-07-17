## Schema Definition Language

Defining `Schema` in IDL/SDL can greatly speed up and simplify code base.

### Load schema from SDL

`SchemaBuilder` class has an extension method for loading types from SDL strings
or GraphQL documents.

Schema

```csharp
#include::xref://tests:graphql.tests/SDL/SdlFacts.cs?s=Tanka.GraphQL.Tests.SDL.SdlFacts.Parse_Document_as_Schema
```

Types

```csharp
#include::xref://tests:graphql.tests/SDL/SdlFacts.cs?s=Tanka.GraphQL.Tests.SDL.SdlFacts.Parse_Document_with_types
```

Custom scalars

```csharp
#include::xref://tests:graphql.tests/SDL/SdlFacts.cs?s=Tanka.GraphQL.Tests.SDL.SdlFacts.Parse_custom_scalar
```

Types can be extended

```csharp
#include::xref://tests:graphql.tests/SDL/SdlFacts.cs?s=Tanka.GraphQL.Tests.SDL.SdlFacts.Parse_ObjectType_with_extension
```
