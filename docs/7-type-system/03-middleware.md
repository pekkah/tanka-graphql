## Schema Builder Middleware

The schema builder uses a middleware pipeline architecture that allows for modular and extensible schema construction. Each stage of the schema building process is handled by dedicated middleware components that can be customized, replaced, or extended.

### Middleware Pipeline Overview

The schema building process is divided into several stages, each handled by specific middleware:

1. **Initialization** - Set up basic schema structure and built-in types
2. **TypeCollection** - Gather and organize type definitions 
3. **LinkProcessing** - Process @link directives and import external schemas
4. **TypeResolution** - Apply directives and configure resolvers
5. **Validation** - Validate the complete schema
6. **Finalization** - Create the final executable schema

### Built-in Middleware

#### BuiltInTypesMiddleware
Adds standard GraphQL built-in types (String, Int, Float, Boolean, ID) and core directives (@include, @skip, @deprecated, etc.) to every schema.

#### LinkProcessingMiddleware
Processes @link directives to import types and directives from external schemas. This enables Apollo Federation v2.3 support and schema composition.

#### ApplyDirectivesMiddleware
Applies directive visitors to transform types and fields based on directive usage. This is where custom directive logic is executed.

#### IntrospectionMiddleware
Adds GraphQL introspection types and fields (__schema, __type, etc.) that enable schema introspection queries.

#### ValidationMiddleware
Validates the schema for consistency, completeness, and GraphQL specification compliance.

#### FinalizationMiddleware
Creates the final executable schema with all types, resolvers, and configuration properly assembled.

### Custom Middleware

You can create custom middleware by implementing the `ISchemaBuildMiddleware` interface:

```csharp
public class CustomMiddleware : ISchemaBuildMiddleware
{
    public async Task<ISchema> InvokeAsync(ISchemaBuildContext context, SchemaBuildDelegate next)
    {
        // Pre-processing logic
        Console.WriteLine($"Processing schema at stage: {context.Stage}");
        
        // Call next middleware in pipeline
        var schema = await next(context);
        
        // Post-processing logic
        Console.WriteLine("Schema processing completed");
        
        return schema;
    }
}
```

### Configuring the Pipeline

The middleware pipeline can be customized through `SchemaBuildOptions`:

```csharp
var schema = new SchemaBuilder()
    .Add(schemaSDL)
    .Build(options =>
    {
        // Add custom middleware at specific stage
        options.Use(SchemaBuildStage.TypeResolution, new CustomMiddleware());
        
        // Configure other options
        options.Resolvers = resolvers;
        options.Subscribers = subscribers;
    });
```

### Extension Methods

Common middleware configurations are available as extension methods:

```csharp
var schema = new SchemaBuilder()
    .Add(schemaSDL)
    .Build(options =>
    {
        options.Resolvers = resolvers;
        
        // Add Apollo Federation support
        options.AddApolloFederation(subgraphOptions);
        
        // Add custom directive visitor
        options.AddDirectiveVisitor<MyDirectiveVisitor>("myDirective");
    });
```

### Apollo Federation Integration

Apollo Federation support is implemented as middleware that integrates seamlessly into the pipeline:

```csharp
// Federation middleware is automatically added when using AddApolloFederation
var schema = new ExecutableSchemaBuilder()
    .Add(federatedSchemaSDL)
    .AddApolloFederation(new SubgraphOptions(referenceResolvers))
    .Build();
```

The federation middleware:
- Processes @link directives in LinkProcessing stage
- Imports required Federation types and directives  
- Adds _service and _entities fields in TypeResolution stage
- Configures entity resolution based on reference resolvers

### Best Practices

1. **Keep middleware focused** - Each middleware should have a single, well-defined responsibility
2. **Handle errors gracefully** - Middleware should validate inputs and provide meaningful error messages
3. **Preserve context** - Pass context information between middleware stages when needed
4. **Order matters** - Consider the execution order when adding custom middleware
5. **Test thoroughly** - Middleware affects the entire schema building process, so comprehensive testing is essential

### Troubleshooting

#### Common Issues

- **Middleware not executing**: Check that middleware is added to the correct stage
- **Type conflicts**: Ensure middleware doesn't add duplicate types or directives
- **Resolution failures**: Verify that middleware properly configures resolvers and type mappings
- **Performance issues**: Profile middleware execution to identify bottlenecks

#### Debug Mode

Enable detailed logging to troubleshoot middleware issues:

```csharp
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Debug);
});
```

The middleware pipeline provides a powerful and flexible way to extend Tanka GraphQL's schema building capabilities while maintaining clean separation of concerns and testability.