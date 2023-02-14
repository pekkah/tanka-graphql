## Schema

> [Specification](https://facebook.github.io/graphql/June2018/#sec-Schema)

Schema is defined by `ISchema`-interface.

```csharp
#include::xref://src:GraphQL/TypeSystem/ISchema.cs
```

### Defaults

By default following type system is included when building a schema using `SchemaBuilder`.

```csharp
#include::xref://src:GraphQL/TypeSystem/SchemaBuilder.cs?s=Tanka.GraphQL.SchemaBuilder.BuiltInTypes
```