## Schema Introspection

> [Specification](https://spec.graphql.org/draft/#sec-Schema-Introspection)

Introspecting a schema produces an executable schema. Queries can be executed against this schema normally.

Create and introspect a schema

```csharp
#include::xref://tests:GraphQL.Tests/Introspection/IntrospectSchemaFacts.cs?s=Tanka.GraphQL.Tests.Introspection.IntrospectSchemaFacts.IntrospectSchemaFacts
```

Combination of the source schema and the introspection schema can be used to provide the normal executable schema with introspection support.

### Type Kinds

Scalar

```csharp
#include::xref://tests:GraphQL.Tests/Introspection/IntrospectSchemaFacts.cs?s=Tanka.GraphQL.Tests.Introspection.IntrospectSchemaFacts.Type_ScalarType
```

Object type

```csharp
#include::xref://tests:GraphQL.Tests/Introspection/IntrospectSchemaFacts.cs?s=Tanka.GraphQL.Tests.Introspection.IntrospectSchemaFacts.Type_ObjectType
```

Union type

```csharp
#include::xref://tests:GraphQL.Tests/Introspection/IntrospectSchemaFacts.cs?s=Tanka.GraphQL.Tests.Introspection.IntrospectSchemaFacts.Type_UnionType
```

Interface type

```csharp
#include::xref://tests:GraphQL.Tests/Introspection/IntrospectSchemaFacts.cs?s=Tanka.GraphQL.Tests.Introspection.IntrospectSchemaFacts.Type_InterfaceType
```

Enum type

```csharp
#include::xref://tests:GraphQL.Tests/Introspection/IntrospectSchemaFacts.cs?s=Tanka.GraphQL.Tests.Introspection.IntrospectSchemaFacts.Type_EnumType
```

Input object

```csharp
#include::xref://tests:GraphQL.Tests/Introspection/IntrospectSchemaFacts.cs?s=Tanka.GraphQL.Tests.Introspection.IntrospectSchemaFacts.Type_InputObjectType
```
