## Schema Builder

The `SchemaBuilder` class is a builder for `ISchema` instances. It provides methods to add various components to the schema and then build the schema.

## Properties

- `BuiltInTypes`: A static property that returns the built-in types of the schema.
- `BuiltInTypeNames`: A static property that returns the names of the built-in types.

## Methods

- `Add(TypeSystemDocument typeSystem)`: Adds a type system document to the schema.
- `Add(string typeSystemSdl)`: Adds a type system SDL to the schema.
- `Add(SchemaDefinition schemaDefinition)`: Adds a schema definition to the schema.
- `Add(SchemaExtension schemaExtension)`: Adds a schema extension to the schema.
- `Add(TypeDefinition typeDefinition)`: Adds a type definition to the schema.
- `Add(TypeDefinition[] typeDefinitions)`: Adds multiple type definitions to the schema.
- `Add(DirectiveDefinition directiveDefinition)`: Adds a directive definition to the schema.
- `Add(DirectiveDefinition[] directiveDefinitions)`: Adds multiple directive definitions to the schema.
- `Add(TypeExtension typeExtension)`: Adds a type extension to the schema.
- `Add(TypeExtension[] typeExtensions)`: Adds multiple type extensions to the schema.
- `Build(IResolverMap resolvers, ISubscriberMap? subscribers = null)`: Builds the schema with the provided resolvers and subscribers.
- `QueryTypeDefinitions(Func<TypeDefinition, bool> filter, SchemaBuildOptions? options = null)`: Queries the type definitions in the schema with a filter function.
- `Build(SchemaBuildOptions options)`: Builds the schema with the provided build options.

The `Build` method is used to create an instance of `ISchema` with the added components.
