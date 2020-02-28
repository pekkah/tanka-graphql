## Imports

GraphQL specification does not yet provide a way of importing types
from SDL from other sources. There is some [discussion][] on
this topic but nothing concrete and "official" is yet released. 

> [graphql-import][] solves this, but the syntax does not feel like
> GraphQL and also would require additional syntax support from 
> parsers.

Tanka GraphQL solves this problem by providing a type system
level directive which allows specifying path and list of types 
to import. The actual details of how the import is fulfilled is
delegated to import providers. Few providers are provided out of the box
and others can be added.


### Syntax

GraphQL does not allows specifying type system level directives.
TGQL goes around this limitation by putting them inside a comment
in the beginning of SDL content. Parser will check each comment line
in the beginning of the SDL for `@import` directive.

Example import

```graphql
# @import(path: "/query")
```

Import directive is defined as path and list of types. On part omitted
as GraphQL does not provide a suitable target yet.

```graphql
directive @import(
    path: String!
    types: [String!]
)
```

> Filtering using the `types` is not yet supported. All types will be
> imported.


### Providers

`Parser.ParseDocumentAsync` allows providing `ParserOptions` which allows 
setting a list of import providers. Parser will query these providers when
it finds the `@import` directives. Provider can tell the parser that it can
complete the import by returning true from `CanImport` function. If multiple
providers can import the import then the parser will pick the first one.

Built in providers: (these are added to options by default)
* `ExtensionsImportProvider`: provides Tanka GraphQL extension types,
* `FileSystemImportProvider`: import types from files,
* `EmbeddedResourceImportProvider`: import types from EmbeddedResources.

Custom providers can be implemented by implementing `IImportProvider` interface
and adding the provider to the options.

> Imports are only supported when using the `SchemaBuilder.SdlAsync` extension 
> method or `Parser.ParseDocumentAsync` 

#### `ExtensionsImportProvider`

This import provider allows importing Tanka GraphQL extensions. Currently these
extensions only include:
* [cost-analysis][]: `@cost` directive.

Syntax
```graphql
# @import(path: "tanka://<extension>")
```


#### `FileSystemImportProvider`

This import provider allows importing files. These files are parsed using the 
same parser options as the main file and can also contain other imports.

Syntax
```graphql
# @import(path: "path/to/file")
```
If no file extension provided then ".graphql" will be appended to the path.

Example
[{Tanka.GraphQL.Tests.Language.ImportProviders.FileSystemImportFacts.Parse_Sdl}]


#### `EmbeddedResourceImportProvider`

This import provider allows importing files embedded into the assembly. These files are parsed using the 
same parser options as the main file and can also contain other imports.

Syntax
```graphql
# @import(path: "embedded://<assembly>/<resourceName>")
```


### Other Examples

See [cost-analysis][] for example import from the TGQL extensions.

[discussion]: https://github.com/graphql/graphql-wg/blob/master/notes/2018-02-01.md#present-graphql-import
[graphql-import]: https://github.com/ardatan/graphql-import
[cost-analysis]: 5-extensions/5-query-cost-analysis.html
