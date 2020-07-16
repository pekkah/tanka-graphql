## Imports

Todo: fix syntax

GraphQL specification does not yet provide a way of importing types
from SDL from other sources. There is some [discussion][] on
this topic but nothing concrete and "official" is yet released.

> [graphql-import][] solves this with a JS style syntax

Tanka GraphQL solves this by providing a similar syntax to
[graphql-import][] but not implementing it fully.

### Syntax

Syntax of the import requires providing a keyword, optional type filters
and the location from which to import. Location does not need to be a file
system.

Example: import all types from "/query"

```graphql
tanka_import from "/query"
```

Example: import `Person` from `"/types/Person"`

```graphql
tanka_import Person from "/types/Person"
```

Example: import `Person`, `Pet` from `"/types/Person"`

```graphql
tanka_import Person, Pet from "/types/Person"
```

### Providers

`Parser.ParseDocumentAsync` allows providing `ParserOptions` which allows
setting a list of import providers. Parser will query these providers when
it finds the `tanka_import`-keyword. Provider can tell the parser that it can
complete the import by returning true from `CanImport` function. If multiple
providers can import the import then the parser will pick the first one.

Built in providers: (these are added to options by default)

- `ExtensionsImportProvider`: provides Tanka GraphQL extension types,
- `FileSystemImportProvider`: import types from files,
- `EmbeddedResourceImportProvider`: import types from EmbeddedResources.

Custom providers can be implemented by implementing `IImportProvider` interface
and adding the provider to the options.

> Imports are only supported when using the `SchemaBuilder.SdlAsync` extension
> method or `Parser.ParseDocumentAsync`

#### `ExtensionsImportProvider`

This import provider allows importing Tanka GraphQL extensions. Currently these
extensions only include:

- [cost-analysis][]: `@cost` directive.

Syntax

```graphql
tanka_import from "tanka://<extension>"
```

#### `FileSystemImportProvider`

This import provider allows importing files. These files are parsed using the
same parser options as the main file and can also contain other imports.

Syntax

```graphql
tanka_import from "path/to/file"
```

If no file extension provided then ".graphql" will be appended to the path.

Example
[{Tanka.GraphQL.Tests.Language.ImportProviders.FileSystemImportFacts.Parse_Sdl}]

```csharp
#include::xref://tests:graphql.tests/Language/ImportProviders/FileSystemImportFacts.cs?s=Tanka.GraphQL.Tests.Language.ImportProviders.FileSystemImportFacts.Parse_Sdl
```

#### `EmbeddedResourceImportProvider`

This import provider allows importing files embedded into the assembly. These files are parsed using the
same parser options as the main file and can also contain other imports.

Syntax

```graphql
tanka_import from "embedded://<assembly>/<resourceName>"
```

### Other Examples

See [cost-analysis][] for example import from the TGQL extensions.

[discussion]: https://github.com/graphql/graphql-wg/blob/master/notes/2018-02-01.md#present-graphql-import
[graphql-import]: https://github.com/ardatan/graphql-import
[cost-analysis]: 5-extensions/5-query-cost-analysis.html
