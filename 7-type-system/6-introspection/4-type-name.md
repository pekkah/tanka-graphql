## Type Name Introspection

> [Specification](https://facebook.github.io/graphql/June2018/#sec-Type-Name-Introspection)

Any query can include the `__typename` meta-field. It returns the name of the object type currently being queried.

### Query the actual type name of when querying on interface

```csharp
#include::xref://tests:graphql.tests/StarwarsFacts.cs?s=Tanka.GraphQL.Tests.StarwarsFacts.Query_typename_of_characters
```
