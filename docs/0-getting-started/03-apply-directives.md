## Apply directives

Directives are annotations for your GraphQL schema or operations.

> See also
>
> - [Specification](https://graphql.github.io/graphql-spec/June2018/#sec-Type-System.Directives)
> - [Directives](xref://types:13-directives.md)

Tanka GraphQL provides support for execution and schema directives:

- Execution directives: Include and Skip
- Schema directives

> Currrently schema directives are only supported on fields of object types

### Schema directives

Schema directives can be used to modify field for example by modifying the resolver of the field.

```csharp
#include::xref://tutorials:GettingStarted.cs?s=Tanka.GraphQL.Tutorials.GettingStarted.GettingStarted.Part3_ApplyDirectives_on_Object_fields
```
