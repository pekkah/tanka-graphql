## @link Directive

The `@link` directive enables schema composition by importing types and directives from external schemas. This powerful feature allows you to build modular GraphQL schemas and is the foundation for specifications like Apollo Federation.

### Overview

The `@link` directive is applied to the schema definition and allows you to:
- Import types and directives from external schema specifications
- Use type aliasing to avoid naming conflicts
- Compose schemas from multiple sources
- Enable federation and other advanced GraphQL patterns

### Basic Usage

#include::xref://tests:GraphQL.Tests/LinkDirectiveDocumentationExamples.cs?s=Tanka.GraphQL.Tests.LinkDirectiveDocumentationExamples.BasicLinkUsage

### Syntax

The `@link` directive follows the GraphQL specification for schema composition:

- **url** (required): The URL of the specification to import from
- **as**: Namespace prefix for imported types (e.g., `federation` would prefix types as `federation__Type`)
- **import**: List of specific types and directives to import
- **for**: Purpose of the link (SECURITY or EXECUTION)

### Import Syntax

The `import` parameter accepts several formats:

#### Simple Import
Import types and directives by name using string literals.

#### Import with Aliasing
Rename imported types to avoid conflicts:

#include::xref://tests:GraphQL.Tests/LinkDirectiveDocumentationExamples.cs?s=Tanka.GraphQL.Tests.LinkDirectiveDocumentationExamples.ImportWithAliasing

### How It Works

When the schema builder encounters a `@link` directive:

1. **URL Resolution**: The URL is resolved to load the external schema
2. **Schema Loading**: The appropriate schema loader fetches the specification
3. **Import Filtering**: Only the specified imports (or all if none specified) are included
4. **Type Aliasing**: Imported types are renamed if aliases are provided
5. **Schema Merging**: Imported types are merged into your schema

### Schema Loaders

Tanka GraphQL includes built-in schema loaders:

#### FederationSchemaLoader
Handles Apollo Federation specifications:
```csharp
// Automatically configured when using Federation
options.SchemaLoader = new FederationSchemaLoader();
```

#### HttpSchemaLoader
Loads schemas from HTTP endpoints:
```csharp
options.SchemaLoader = new HttpSchemaLoader();
```

#### CompositeSchemaLoader
Chains multiple loaders:
```csharp
options.SchemaLoader = new CompositeSchemaLoader(
    new FederationSchemaLoader(),
    new HttpSchemaLoader()
);
```

### Examples

#### Importing Federation Types

#include::xref://tests:GraphQL.Tests/LinkDirectiveDocumentationExamples.cs?s=Tanka.GraphQL.Tests.LinkDirectiveDocumentationExamples.ImportingFederationTypes

#### Using Namespaces

#include::xref://tests:GraphQL.Tests/LinkDirectiveDocumentationExamples.cs?s=Tanka.GraphQL.Tests.LinkDirectiveDocumentationExamples.UsingNamespaces

#### Custom Specifications

You can import from your own specifications:

#include::xref://tests:GraphQL.Tests/LinkDirectiveDocumentationExamples.cs?s=Tanka.GraphQL.Tests.LinkDirectiveDocumentationExamples.CustomSpecifications

### Creating Custom Schema Loaders

Implement `ISchemaLoader` to load schemas from custom sources:

#include::xref://tests:GraphQL.Tests/LinkDirectiveDocumentationExamples.cs?s=Tanka.GraphQL.Tests.LinkDirectiveDocumentationExamples.CustomSchemaLoaderExample

### Processing Pipeline

The `@link` directive is processed by the `LinkProcessingMiddleware` during schema building:

1. **Initialization Stage**: Basic schema setup
2. **Type Collection**: Gather type definitions
3. **Link Processing Stage**: 
   - Process all `@link` directives
   - Load external schemas
   - Apply import filtering
   - Handle type aliasing
   - Merge imported types
4. **Type Resolution**: Configure resolvers
5. **Validation**: Validate complete schema
6. **Finalization**: Create executable schema

### Best Practices

1. **Be Specific with Imports**: Only import what you need to avoid namespace pollution

#include::xref://tests:GraphQL.Tests/LinkDirectiveDocumentationExamples.cs?s=Tanka.GraphQL.Tests.LinkDirectiveDocumentationExamples.SpecificImportsBestPractice

2. **Use Aliasing for Conflicts**: When importing from multiple sources, use aliases to avoid naming conflicts
3. **Version Your Specifications**: Include version numbers in URLs for stability

#include::xref://tests:GraphQL.Tests/LinkDirectiveDocumentationExamples.cs?s=Tanka.GraphQL.Tests.LinkDirectiveDocumentationExamples.VersionedSpecificationsBestPractice

4. **Cache External Schemas**: Implement caching in custom loaders for performance
5. **Validate Imports**: Ensure imported types are used correctly in your schema

### Troubleshooting

#### Common Issues

- **Schema not loading**: Verify the URL is accessible and the schema loader is configured
- **Type conflicts**: Use aliasing or namespaces to resolve naming conflicts
- **Missing imports**: Check that all required types are included in the import list
- **Circular dependencies**: Avoid schemas that link to each other recursively

#### Debugging

Enable debug logging to trace @link processing by configuring logging at the Debug level in your application.

### Limitations

- Type aliasing currently only supports renaming at import time
- Recursive linking depth is limited to prevent infinite loops
- Schema loaders must return valid GraphQL SDL

### See Also

- [Middleware Pipeline](03-middleware.md) - How @link processing fits in the build pipeline
- [Apollo Federation](../10-extensions/apollo-federation.md) - Using @link for federation
- [Schema Builder](01_1-builder.md) - Configuring schema loaders