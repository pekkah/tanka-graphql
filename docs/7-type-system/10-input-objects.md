## Input Objects

> [Specification](https://spec.graphql.org/draft/#sec-Input-Objects)

Input objects are created as instances of `InputObjectDefinition`.

### @oneOf Directive

The `@oneOf` directive can be applied to input objects to indicate that exactly one field must be provided. This is useful for creating polymorphic input types where only one variant should be specified.

> **Note**: The @oneOf directive is currently at Stage 3 (RFC Accepted) in the GraphQL specification process. See the [RFC](https://github.com/graphql/graphql-spec/pull/825) for more details.

```graphql
input SearchInput @oneOf {
  byName: String
  byId: ID
  byEmail: String
}

type Query {
  search(input: SearchInput!): [User!]!
}
```

With this definition, valid queries would include:
```graphql
# Valid - exactly one field
{ search(input: { byName: "John" }) }
{ search(input: { byId: "123" }) }

# Invalid - multiple fields
{ search(input: { byName: "John", byId: "123" }) }

# Invalid - no fields
{ search(input: {}) }

# Invalid - null value
{ search(input: { byName: null }) }
```

### Examples

```csharp
#include::xref://tests:GraphQL.Language.Tests/Nodes/InputObjectDefinitionFacts.cs
```
