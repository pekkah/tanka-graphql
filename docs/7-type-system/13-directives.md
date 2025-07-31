## Directives

> [Specification](https://spec.graphql.org/draft/#sec-Type-System.Directives)

Directive types are created as instance of `DirectiveDefinition`. When used with other types instances of `Directive` are used.

Built-in directives for executable documents are:
- `@include(if: Boolean!)`: Only include this field in the result if the argument is true.
- `@skip(if: Boolean!)`: Skip this field if the argument is true.

Built-in directives for schema documents are:
- `@deprecated(reason: String)`: Marks an element of a GraphQL schema as no longer supported.
- `@specifiedBy(url: String!)`: Exposes a URL that specifies the behaviour of this scalar.
- `@oneOf`: Indicates that exactly one field must be provided in input objects. *(Stage 3 RFC)*

For custom schema directive see [Apply Directives](xref://start:03-apply-directives.md) for example.

### Examples

```csharp
#include::xref://tests:GraphQL.Language.Tests/Nodes/DirectiveDefinitionFacts.cs
```

```csharp
#include::xref://tests:GraphQL.Language.Tests/Nodes/DirectiveFacts.cs
```