## Imports

GraphQL specfication does not yet provide a way of importing types
in SDL from other sources. There is some [discussion][] on
this topic but nothing concrete and "official" is yet released. 

> [graphql-import][] solves this, but the syntax does not feel like
> GraphQL and also would require additional syntax support from 
> parsers.

Tanka GraphQL solves this problem by providing a type system
level directive which allows specifying path and list of types 
to import. The actual details of how the import is fulfilled is
delegated to providers. Few providers are provided out of the box
and others can be added.


### Syntax

GraphQL does not allows specifying type system level directives.
Because of this TGQL implements them by looking them from the comment
lines in the beginning of SDL content.

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


### Providers

`Parser.ParseDocumentAsync` allows providing `ParserOptions` which allows 
setting a list of import providers. Parser will query these providers when
it finds the `@import` directives. Provider can tell the parser that it can
complete the import by returning true from `CanImport` function. If multiple
providers can import the import then the parser will pick the first one.

Built in providers: (these are added to options by default)
* ExtensionsImportProvider: provides Tanka GraphQL extension types,
* FileSystemImportProvider: import types from files,
* EmbeddedResourceImportProvider: import types from EmbeddedResources.

Custom providers can be implemented by implementing `IImportProvider` interface
and adding the provider to the options.


[discussion]: https://github.com/graphql/graphql-wg/blob/master/notes/2018-02-01.md#present-graphql-import
[graphql-import]: https://github.com/ardatan/graphql-import