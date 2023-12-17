## Executable Schema Builder

The `ExecutableSchemaBuilder` class is a builder for `ISchema` instances. It extends the capabilities of `SchemaBuilder` with the ability to add resolvers, subscribers and value converters required during execution.

## Properties

- `Schema`: A `SchemaBuilder` instance used to build the schema.
- `Resolvers`: A `ResolversBuilder` instance used to build the resolvers.
- `ValueConverters`: A `ValueConvertersBuilder` instance used to build the value converters.
- `DirectiveVisitorFactories`: A dictionary of directive visitor factories.

## Methods

- `Add(TypeSystemDocument document)`: Adds a type system document to the schema.
- `Add(IExecutableSchemaConfiguration configuration)`: Adds a schema configuration to the schema.
- `Add(IResolverMap resolverMap)`: Adds a resolver map to the schema.
- `Add(TypeDefinition[] types)`: Adds multiple type definitions to the schema.
- `Add(string typeName, FieldsWithResolvers fields, FieldsWithSubscribers? subscribers = null)`: Adds a type with its fields and optional subscribers to the schema.
- `AddConverter(string typeName, IValueConverter valueConverter)`: Adds a value converter for a specific type to the schema.
- `Build(Action<SchemaBuildOptions>? configureBuildOptions = null)`: Builds the schema with the provided build options.

The `Build` method is used to create an instance of `ISchema` with the added components.
