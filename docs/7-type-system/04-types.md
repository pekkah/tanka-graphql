## Types

> [Specification](https://facebook.github.io/graphql/June2018/#sec-Types)

Following types are supported:

- `ScalarDefinition`
- `ObjectDefinition`
- `InterfaceDefinition`
- `UnionDefinition`
- `EnumDefinition`
- `InputObjectDefinition`

These all exists in `Tanka.GraphQL.Language.Nodes.TypeSystem` namespace and implement `TypeDefinition` base class.

### Examples of creating type instances

Following shows how to create `ScalarDefinition` from bytes or string. Other types are created in similar way. You can also create actual instance of the type class but that can get quite verbose 
for more complicated types.

```csharp
#include::xref://tests:GraphQL.Language.Tests/Nodes/ScalarDefinitionFacts.cs?s=Tanka.GraphQL.Language.Tests.Nodes.ScalarDefinitionFacts.FromBytes

#include::xref://tests:GraphQL.Language.Tests/Nodes/ScalarDefinitionFacts.cs?s=Tanka.GraphQL.Language.Tests.Nodes.ScalarDefinitionFacts.FromString
```