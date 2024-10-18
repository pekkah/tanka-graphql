## Language

Tanka GraphQL implements lexer and parser for GraphQL. Implementation
is done following the latest [draft][draft] specification (2020).

Lexer reads stream of tokens from UTF8 byte array ([ReadOnlySpan<byte>][readonlyspan<byte>])
and parser turns these tokens into syntax tree.

### Example: Parser usage

```csharp
#include::xref://tests:GraphQL.Language.Tests/ParserFacts.cs?s=Tanka.GraphQL.Language.Tests.ParserFacts.ExecutableDocument
```

### Example: Parser usage simplified

```csharp
#include::xref://tests:GraphQL.Language.Tests/Nodes/ObjectDefinitionFacts.cs?s=Tanka.GraphQL.Language.Tests.Nodes.ObjectDefinitionFacts.FromString
```

### Example: Implicit Conversion from String to ExecutableDocument

```csharp
ExecutableDocument query = """
{
    field1
    field2
}
""";
```

[draft]: http://spec.graphql.org/draft/
[readonlyspan<byte>]: https://docs.microsoft.com
