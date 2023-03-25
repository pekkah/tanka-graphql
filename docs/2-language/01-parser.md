## Language

Tanka GraphQL implements lexer and parser for GraphQL. Implementation
is done following the latest [draft][draft] specification (2020).

Lexer reads stream of tokens from UTF8 byte array ([ReadOnlySpan<byte>][readonlyspan<byte>])
and parser turns these tokens into syntax tree.

### Example: Parser usage

```csharp
#include::xref://tests:graphql.language.tests/ParserFacts.cs?s=Tanka.GraphQL.Language.Tests.ParserFacts.ExecutableDocument
```

### Example: Parser usage simplified

```csharp
#include::xref://tests:graphql.language.tests/Nodes/ObjectDefinitionFacts.cs?s=Tanka.GraphQL.Language.Tests.Nodes.ObjectDefinitionFacts.FromString
```

[draft]: http://spec.graphql.org/draft/
[readonlyspan<byte>]: https://docs.microsoft.com
