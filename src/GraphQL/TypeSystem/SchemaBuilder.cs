using System.Collections.Concurrent;

using Tanka.GraphQL.Directives;
using Tanka.GraphQL.Introspection;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem.SchemaValidation;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.ValueSerialization;

namespace Tanka.GraphQL.TypeSystem;

public class SchemaBuilder
{
    private static readonly Dictionary<string, FieldDefinition> NoFields = new(0);
    private static readonly Dictionary<string, InputValueDefinition> NoInputObjectFields = new(0);
    private static readonly List<Directive> NoDirectives = new(0);
    private readonly ConcurrentDictionary<string, DirectiveDefinition> _directiveDefinitions = new();


    private readonly ConcurrentBag<SchemaDefinition> _schemaDefinitions = new();
    private readonly ConcurrentBag<SchemaExtension> _schemaExtensions = new();

    private readonly ConcurrentDictionary<string, TypeDefinition> _typeDefinitions = new();
    private readonly ConcurrentDictionary<string, ConcurrentBag<TypeExtension>> _typeExtensions = new();

    /// <summary>
    ///     Default type system with built-in types
    /// </summary>
    public static TypeSystemDocument BuiltInTypes => @"
""""""
The `Boolean` scalar type represents `true` or `false`
""""""
scalar Boolean

""""""
The `Float` scalar type represents signed double-precision fractional values
as specified by '[IEEE 754](http://en.wikipedia.org/wiki/IEEE_floating_point)
""""""
scalar Float

""""""
The ID scalar type represents a unique identifier, often used to refetch an object
or as the key for a cache. The ID type is serialized in the same way as a String;
however, it is not intended to be human‐readable. While it is often numeric, it
should always serialize as a String.
""""""
scalar ID

""""""
The `Int` scalar type represents non-fractional signed whole numeric values
""""""
scalar Int

""""""
The `String` scalar type represents textual data, represented as UTF-8
character sequences. The String type is most often used by GraphQL to
represent free-form human-readable text.
""""""
scalar String

directive @deprecated(reason: String) on
    | FIELD_DEFINITION
    | ENUM_VALUE

directive @include(if: Boolean!) on
    | FIELD
    | FRAGMENT_SPREAD
    | INLINE_FRAGMENT

directive @skip(if: Boolean!) on
    | FIELD
    | FRAGMENT_SPREAD
    | INLINE_FRAGMENT

directive @specifiedBy(url: String!) on SCALAR

directive @oneOf on INPUT_OBJECT

directive @link(
  url: String!,
  as: String,
  import: [link__Import],
  for: link__Purpose
) repeatable on SCHEMA

scalar link__Import

enum link__Purpose {
  SECURITY
  EXECUTION
}
";

    /// <summary>
    ///     Get list of built-in type names
    /// </summary>
    public static IReadOnlyList<string> BuiltInTypeNames = BuiltInTypes.GetNamedTypes()
        .Select(n => n.Name.Value)
        .ToList();

    /// <summary>
    ///     Add types from <see cref="TypeSystemDocument" /> into the builder
    /// </summary>
    /// <param name="typeSystem"></param>
    /// <returns></returns>
    public SchemaBuilder Add(TypeSystemDocument typeSystem)
    {

        if (typeSystem.SchemaDefinitions is not null)
            foreach (var schemaDefinition in typeSystem.SchemaDefinitions)
                Add(schemaDefinition);

        if (typeSystem.SchemaExtensions is not null)
            foreach (var schemaExtension in typeSystem.SchemaExtensions)
                Add(schemaExtension);

        // TODO: Process @link directives from schema definitions and extensions
        // This will be implemented when we add proper schema linking functionality

        if (typeSystem.TypeDefinitions is not null)
            foreach (var typeDefinition in typeSystem.TypeDefinitions)
                Add(typeDefinition);

        if (typeSystem.TypeExtensions is not null)
            foreach (var typeExtension in typeSystem.TypeExtensions)
                Add(typeExtension);

        if (typeSystem.DirectiveDefinitions is not null)
            foreach (var directiveDefinition in typeSystem.DirectiveDefinitions)
                Add(directiveDefinition);

        return this;
    }

    /// <summary>
    ///     Parse type system document from SDL and add types into the builder
    /// </summary>
    /// <param name="typeSystemSdl"></param>
    /// <returns></returns>
    public SchemaBuilder Add(string typeSystemSdl)
    {
        return Add((TypeSystemDocument)typeSystemSdl);
    }

    /// <summary>
    ///     Add schema definition into the builder
    /// </summary>
    /// <param name="schemaDefinition"></param>
    /// <returns></returns>
    public SchemaBuilder Add(SchemaDefinition schemaDefinition)
    {
        _schemaDefinitions.Add(schemaDefinition);
        return this;
    }

    /// <summary>
    ///     Add schema extension into the builder
    /// </summary>
    /// <param name="schemaExtension"></param>
    /// <returns></returns>
    public SchemaBuilder Add(SchemaExtension schemaExtension)
    {
        _schemaExtensions.Add(schemaExtension);
        return this;
    }

    /// <summary>
    ///     Add type definition into the builder
    /// </summary>
    /// <param name="typeDefinition"></param>
    public SchemaBuilder Add(TypeDefinition typeDefinition)
    {
        if (!_typeDefinitions.TryAdd(typeDefinition.Name, typeDefinition))
            throw TypeAlreadyExists(typeDefinition.Name);

        return this;
    }

    /// <summary>
    ///     Add type definitions into the builder
    /// </summary>
    /// <param name="typeDefinitions"></param>
    public SchemaBuilder Add(TypeDefinition[] typeDefinitions)
    {
        foreach (var typeDefinition in typeDefinitions)
            Add(typeDefinition);

        return this;
    }

    /// <summary>
    ///     Add directive definition into the builder
    /// </summary>
    /// <param name="directiveDefinition"></param>
    public SchemaBuilder Add(DirectiveDefinition directiveDefinition)
    {
        if (!_directiveDefinitions.TryAdd(directiveDefinition.Name, directiveDefinition))
            throw TypeAlreadyExists(directiveDefinition.Name);

        return this;
    }

    /// <summary>
    ///     Add directive definitions into the builder
    /// </summary>
    /// <param name="directiveDefinitions"></param>
    public SchemaBuilder Add(DirectiveDefinition[] directiveDefinitions)
    {
        foreach (var directiveDefinition in directiveDefinitions)
            Add(directiveDefinition);

        return this;
    }

    /// <summary>
    ///     Add type extension into the builder
    /// </summary>
    /// <param name="typeExtension"></param>
    /// <returns></returns>
    public SchemaBuilder Add(TypeExtension typeExtension)
    {
        var typeExtensions = _typeExtensions
            .GetOrAdd(typeExtension.Name, _ => new());

        typeExtensions.Add(typeExtension);

        return this;
    }

    /// <summary>
    ///     Add type extensions into the builder
    /// </summary>
    /// <param name="typeExtensions"></param>
    /// <returns></returns>
    public SchemaBuilder Add(TypeExtension[] typeExtensions)
    {
        foreach (var typeExtension in typeExtensions) Add(typeExtension);

        return this;
    }

    /// <summary>
    ///     Build schema from the builder
    /// </summary>
    /// <param name="resolvers"></param>
    /// <param name="subscribers"></param>
    /// <returns></returns>
    public Task<ISchema> Build(IResolverMap resolvers, ISubscriberMap? subscribers = null)
    {
        return Build(
            new()
            {
                Resolvers = resolvers,
                Subscribers = subscribers
            });
    }

    /// <summary>
    ///     Query types to be built by the builder
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public IEnumerable<TypeDefinition> QueryTypeDefinitions(
        Func<TypeDefinition, bool> filter,
        SchemaBuildOptions? options = null)
    {
        options ??= new();
        var typeDefinitions = BuildTypeDefinitions(options.BuildTypesFromOrphanedExtensions);
        return typeDefinitions.Where(filter);
    }

    /// <summary>
    ///     Build schema from the builder using the given <paramref name="options"/>
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public async Task<ISchema> Build(SchemaBuildOptions options)
    {
        // Process @link directives if a schema loader is provided
        if (options.ProcessLinkDirectives)
        {
            await ProcessLinkDirectivesAsync(options);
        }

        if (options.IncludeBuiltInTypes) Add(BuiltInTypes);

        var resolvers = new ResolversMap(options.Resolvers ?? ResolversMap.None, options.Subscribers);

        if (options.IncludeIntrospection)
        {
            var introspection = Introspect.Create();
            Add(introspection.TypeSystemDocument);

            resolvers += introspection.Resolvers;
            //todo: refactor introspection
        }

        var typeDefinitions = BuildTypeDefinitions(
            options.BuildTypesFromOrphanedExtensions
        );

        (typeDefinitions, resolvers) = RunDirectiveVisitors(typeDefinitions, options, resolvers);

        // Run schema validation after directive visitors have transformed the schema
        if (options.SchemaValidationRules?.Any() == true)
        {
            var validator = new SchemaValidator(options.SchemaValidationRules);
            var validationResult = validator.Validate(typeDefinitions);

            if (!validationResult.IsValid)
            {
                throw new SchemaValidationException(validationResult.Errors);
            }
        }

        var namedTypeDefinitions = typeDefinitions
            .ToDictionary(type => type.Name.Value, type => type);

        var schemas = BuildSchemas().ToList();
        var operationDefinitions = schemas
            .SelectMany(schema => schema.Operations)
            .ToList();

        var queryRoot = FindQueryRoot(
            namedTypeDefinitions,
            operationDefinitions,
            options.OverrideQueryRootName
        );

        var mutationRoot = FindMutationRoot(
            namedTypeDefinitions,
            operationDefinitions,
            options.OverrideMutationRootName
        );

        var subscriptionRoot = FindSubscriptionRoot(
            namedTypeDefinitions,
            operationDefinitions,
            options.OverrideSubscriptionRootName
        );

        var allFields = namedTypeDefinitions
            .Where(kv => kv.Value is ObjectDefinition)
            .ToDictionary(
                kv => kv.Key,
                kv => ((ObjectDefinition)kv.Value).Fields?.ToDictionary(field => field.Name.Value, field => field) ??
                      NoFields
            );

        var interfaceFields = namedTypeDefinitions
            .Where(kv => kv.Value is InterfaceDefinition)
            .ToDictionary(
                kv => kv.Key,
                kv => ((InterfaceDefinition)kv.Value).Fields?.ToDictionary(field => field.Name.Value, field => field) ??
                      NoFields
            );

        foreach (var (type, fields) in interfaceFields) allFields.Add(type, fields);

        var inputFields = namedTypeDefinitions
            .Where(kv => kv.Value is InputObjectDefinition)
            .ToDictionary(
                kv => kv.Key,
                kv =>
                    ((InputObjectDefinition)kv.Value).Fields?.ToDictionary(field => field.Name.Value, field => field) ??
                    NoInputObjectFields
            );

        var schemaDirectives = schemas
            ?.SelectMany(schema => schema.Directives?.ToList() ?? NoDirectives)
            .ToList();

        ISchema schema = new ExecutableSchema(
            namedTypeDefinitions,
            allFields,
            inputFields,
            _directiveDefinitions.ToDictionary(kv => kv.Key, kv => kv.Value),
            queryRoot,
            resolvers,
            options.ValueConverters ?? new Dictionary<string, IValueConverter>(0),
            mutationRoot,
            subscriptionRoot,
            resolvers,
            schemaDirectives
        );

        return schema;
    }


    private (IReadOnlyList<TypeDefinition> TypeDefinitions, ResolversMap Resolvers) RunDirectiveVisitors(
        IEnumerable<TypeDefinition> typeDefinitions,
        SchemaBuildOptions options,
        ResolversMap resolvers)
    {
        if (options.DirectiveVisitorFactories is null)
            return (typeDefinitions.ToList(), resolvers);

        var visitors = options.DirectiveVisitorFactories
            .Select(factory => new KeyValuePair<string, DirectiveVisitor>(
                factory.Key,
                factory.Value(this)
            ));

        return RunDirectiveVisitors(
            typeDefinitions,
            options,
            visitors,
            resolvers);
    }

    private (IReadOnlyList<TypeDefinition> TypeDefinitions, ResolversMap Resolvers) RunDirectiveVisitors(
        IEnumerable<TypeDefinition> typeDefinitions,
        SchemaBuildOptions options,
        IEnumerable<KeyValuePair<string, DirectiveVisitor>> visitors,
        ResolversMap resolvers)
    {
        var typeDefinitionList = typeDefinitions.ToList();
        if (typeDefinitionList.Count is 0)
            return (typeDefinitionList, resolvers);

        var visitorList = visitors.ToList();
        var types = new List<TypeDefinition>();

        for (var typeIndex = 0; typeIndex < typeDefinitionList.Count; typeIndex++)
        {
            var typeDefinition = typeDefinitionList[typeIndex];
            foreach (var (directiveName, visitor) in visitorList)
            {
                if (visitor.TypeDefinition is not null)
                {
                    if (!typeDefinition.TryGetDirective(directiveName, out var directive))
                        continue;

                    var context = new DirectiveTypeVisitorContext(typeDefinition);

                    var maybeSameContext = visitor.TypeDefinition(directive, context);

                    // type removed
                    if (maybeSameContext is null) continue;

                    typeDefinition = maybeSameContext.TypeDefinition;
                }

                if (typeDefinition is ObjectDefinition objectDefinition)
                    if (visitor.FieldDefinition != null && objectDefinition.Fields is { Count: > 0 })
                    {
                        var fieldsChanged = false;
                        var fields = new List<FieldDefinition>(objectDefinition.Fields.Count);
                        foreach (var fieldDefinition in objectDefinition.Fields)
                        {
                            if (!fieldDefinition.TryGetDirective(directiveName, out var directive))
                                continue;

                            var resolver = options.Resolvers?.GetResolver(typeDefinition.Name, fieldDefinition.Name);
                            var subscriber =
                                options.Subscribers?.GetSubscriber(typeDefinition.Name, fieldDefinition.Name);

                            var hasResolver = resolver is not null;
                            var hasSubscriber = subscriber is not null;

                            var context = new DirectiveFieldVisitorContext(
                                fieldDefinition,
                                resolver,
                                subscriber
                            );

                            var maybeSameContext = visitor.FieldDefinition(directive, context);

                            // field not modified
                            if (maybeSameContext == context)
                            {
                                fields.Add(fieldDefinition);
                                continue;
                            }

                            fieldsChanged = true;

                            // field removed
                            if (maybeSameContext is null)
                                continue;

                            fields.Add(maybeSameContext.Field);

                            if (maybeSameContext.Resolver is not null)
                                resolvers.Replace(typeDefinition.Name, fieldDefinition.Name, maybeSameContext.Resolver);

                            if (maybeSameContext.Subscriber is not null)
                                resolvers.Replace(typeDefinition.Name, fieldDefinition.Name,
                                    maybeSameContext.Subscriber);
                        }

                        if (fieldsChanged)
                            typeDefinition = objectDefinition.WithFields(fields);
                    }
            }

            types.Add(typeDefinition);
        }

        return (types, resolvers);
    }

    private Exception TypeAlreadyExists(Name name)
    {
        return new InvalidOperationException(
            $"Type '{name}' already added.");
    }

    private IEnumerable<SchemaDefinition> BuildSchemas()
    {
        var extensions = _schemaExtensions.ToList();
        var extensionDirectives = extensions
            .Where(e => e.Directives is not null)
            .SelectMany(e => e.Directives!)
            .ToList();

        var extensionOperations = extensions
            .Where(e => e.Operations is not null)
            .SelectMany(e => e.Operations!)
            .ToList();

        foreach (var schemaDefinition in _schemaDefinitions)
        {
            var operations = schemaDefinition.Operations.ToList();
            operations.AddRange(extensionOperations);

            yield return schemaDefinition
                .WithDirectives(schemaDefinition.Directives
                    .Concat(extensionDirectives))
                .WithOperations(operations);
        }
    }

    private ObjectDefinition FindQueryRoot(
        IReadOnlyDictionary<string, TypeDefinition> typeDefinitions,
        IEnumerable<RootOperationTypeDefinition> operationDefinitions,
        string? overrideQueryRootName = null)
    {
        if (!string.IsNullOrEmpty(overrideQueryRootName))
            return (ObjectDefinition)typeDefinitions[overrideQueryRootName];

        var queryNamedType = operationDefinitions
            .SingleOrDefault(op => op.OperationType == OperationType.Query)
            ?.NamedType;

        // by convention
        if (queryNamedType == null && typeDefinitions.TryGetValue("Query", out var queryDefinition))
            return (ObjectDefinition)queryDefinition;

        if (queryNamedType is null)
            throw new InvalidOperationException("Could not find query operation");

        return (ObjectDefinition)typeDefinitions[queryNamedType.Name];
    }

    private ObjectDefinition? FindMutationRoot(
        IReadOnlyDictionary<string, TypeDefinition> typeDefinitions,
        IEnumerable<RootOperationTypeDefinition> operationDefinitions,
        string? overrideMutationRootName = null)
    {
        if (!string.IsNullOrEmpty(overrideMutationRootName))
            return (ObjectDefinition)typeDefinitions[overrideMutationRootName];

        var mutationNamedType = operationDefinitions
            .SingleOrDefault(op => op.OperationType == OperationType.Mutation)
            ?.NamedType;

        // by convention
        if (mutationNamedType == null && typeDefinitions.TryGetValue("Mutation", out var mutationDefinition))
            return (ObjectDefinition)mutationDefinition;

        if (mutationNamedType is null)
            return null;

        return (ObjectDefinition)typeDefinitions[mutationNamedType.Name];
    }

    private ObjectDefinition? FindSubscriptionRoot(
        IReadOnlyDictionary<string, TypeDefinition> typeDefinitions,
        IEnumerable<RootOperationTypeDefinition> operationDefinitions,
        string? overrideSubscriptionRootName = null)
    {
        if (!string.IsNullOrEmpty(overrideSubscriptionRootName))
            return (ObjectDefinition)typeDefinitions[overrideSubscriptionRootName];

        var subscriptionNamedType = operationDefinitions
            .SingleOrDefault(op => op.OperationType == OperationType.Subscription)
            ?.NamedType;

        // by convention
        if (subscriptionNamedType == null &&
            typeDefinitions.TryGetValue("Subscription", out var subscriptionDefinition))
            return (ObjectDefinition)subscriptionDefinition;

        if (subscriptionNamedType is null)
            return null;

        return (ObjectDefinition)typeDefinitions[subscriptionNamedType.Name];
    }

    private IEnumerable<TypeDefinition> BuildTypeDefinitions(
        bool buildTypeFromOrphanedExtension)
    {
        var typesToBuild = _typeDefinitions.Values.ToList();
        var typeExtensionsToBuild = _typeExtensions
            .ToDictionary(
                types => types.Key,
                types => types.Value.ToList());


        foreach (var type in typesToBuild)
            if (typeExtensionsToBuild.TryGetValue(type.Name, out var extensions))
            {
                yield return type.Extend(extensions.ToArray());
                typeExtensionsToBuild.Remove(type.Name);
            }
            else
            {
                yield return type;
            }

        // build types from orphaned extensions
        if (buildTypeFromOrphanedExtension && typeExtensionsToBuild.Count > 0)
            foreach (var typeExtension in typeExtensionsToBuild.ToList())
            {
                var (_, extensions) = typeExtension;

                if (extensions.Count == 0)
                    continue;

                // pick first extension as the type
                var type = extensions[0].Definition;
                extensions.RemoveAt(0);

                yield return type.Extend(extensions.ToArray());
            }
    }

    /// <summary>
    /// Process @link directives from all schema definitions and extensions
    /// </summary>
    private async Task ProcessLinkDirectivesAsync(SchemaBuildOptions options)
    {
        var processedUrls = new HashSet<string>();
        var hasNewLinks = true;
        var depth = 0;

        while (hasNewLinks && depth < options.MaxLinkDepth)
        {
            hasNewLinks = false;
            var currentSchemaDefinitions = _schemaDefinitions.ToList();
            var currentSchemaExtensions = _schemaExtensions.ToList();

            var linkInfos = LinkDirectiveProcessor.ProcessLinkDirectives(
                currentSchemaDefinitions,
                currentSchemaExtensions);

            foreach (var linkInfo in linkInfos)
            {
                if (processedUrls.Add(linkInfo.Url)) // Returns true if newly added
                {
                    var linkedSchema = await LoadAndResolveSchemaAsync(
                        linkInfo,
                        processedUrls,
                        options,
                        depth + 1);

                    if (linkedSchema != null)
                    {
                        Add(linkedSchema);
                        hasNewLinks = true;
                    }
                }
            }

            depth++;
        }

        if (depth >= options.MaxLinkDepth)
            throw new InvalidOperationException($"Maximum link depth of {options.MaxLinkDepth} exceeded while processing @link directives");
    }

    /// <summary>
    /// Load and resolve a schema with its dependencies (depth-first)
    /// </summary>
    private async Task<TypeSystemDocument?> LoadAndResolveSchemaAsync(
        LinkInfo linkInfo,
        HashSet<string> visitedUrls,
        SchemaBuildOptions options,
        int depth)
    {
        if (depth >= options.MaxLinkDepth)
            throw new InvalidOperationException($"Maximum link depth exceeded for: {linkInfo.Url}");

        if (options.SchemaLoader == null)
            throw new InvalidOperationException("SchemaLoader is required for processing @link directives");

        // Load the raw schema
        var schema = await options.SchemaLoader.LoadSchemaAsync(linkInfo.Url);
        if (schema == null)
            return null;

        // Extract its @link directives
        var nestedLinkInfos = LinkDirectiveProcessor.ProcessLinkDirectives(
            schema.SchemaDefinitions,
            schema.SchemaExtensions);

        // Process each linked schema FIRST (depth-first)
        var linkedSchemas = new List<(LinkInfo info, TypeSystemDocument doc)>();
        foreach (var nestedLinkInfo in nestedLinkInfos)
        {
            if (!visitedUrls.Contains(nestedLinkInfo.Url))
            {
                visitedUrls.Add(nestedLinkInfo.Url);
                var linkedDoc = await LoadAndResolveSchemaAsync(
                    nestedLinkInfo,
                    visitedUrls,
                    options,
                    depth + 1);

                if (linkedDoc != null)
                    linkedSchemas.Add((nestedLinkInfo, linkedDoc));
            }
        }

        // Build merged document: dependencies first, then current schema
        TypeSystemDocument? mergedDocument = null;

        // Add filtered linked schemas
        foreach (var (nestedLinkInfo, linkedDoc) in linkedSchemas)
        {
            var filtered = ApplyImportFiltering(linkedDoc, nestedLinkInfo);
            if (mergedDocument == null)
                mergedDocument = filtered;
            else
                mergedDocument = mergedDocument.WithTypeSystem(filtered);
        }

        // Apply import filtering to the current schema based on original linkInfo
        var currentFiltered = ApplyImportFiltering(schema, linkInfo);

        // Add current schema last (so it can reference linked types)
        if (mergedDocument == null)
            mergedDocument = currentFiltered;
        else
            mergedDocument = mergedDocument.WithTypeSystem(currentFiltered);

        return mergedDocument;
    }

    /// <summary>
    /// Apply import filtering based on the LinkInfo imports list
    /// </summary>
    private TypeSystemDocument ApplyImportFiltering(TypeSystemDocument document, LinkInfo linkInfo)
    {
        // If no imports specified, return everything except types/directives that already exist
        if (linkInfo.Imports == null || linkInfo.Imports.Count == 0)
        {
            // Filter out types that already exist to avoid duplicates
            var filteredTypes = document.TypeDefinitions?.Where(t => !_typeDefinitions.ContainsKey(t.Name.Value)).ToList();
            var filteredDirectives = document.DirectiveDefinitions?.Where(d => !_directiveDefinitions.ContainsKey(d.Name.Value)).ToList();

            return new TypeSystemDocument(
                document.SchemaDefinitions,
                filteredTypes?.Count > 0 ? filteredTypes : null,
                filteredDirectives?.Count > 0 ? filteredDirectives : null,
                document.SchemaExtensions,
                document.TypeExtensions
            );
        }

        var importedTypes = new List<TypeDefinition>();
        var importedDirectives = new List<DirectiveDefinition>();

        foreach (var import in linkInfo.Imports)
        {
            // TODO: Handle aliasing with LinkImport objects
            // For now, just handle simple string imports

            if (import.StartsWith("@"))
            {
                // Import directive
                var directiveName = import.Substring(1);
                var directive = document.DirectiveDefinitions?.FirstOrDefault(d => d.Name.Value == directiveName);
                if (directive != null && !_directiveDefinitions.ContainsKey(directiveName))
                {
                    importedDirectives.Add(directive);
                }
            }
            else
            {
                // Import type
                var type = document.TypeDefinitions?.FirstOrDefault(t => t.Name.Value == import);
                if (type != null && !_typeDefinitions.ContainsKey(import))
                {
                    importedTypes.Add(type);
                }
            }
        }

        return new TypeSystemDocument(
            document.SchemaDefinitions, // Keep schema definitions
            importedTypes.Count > 0 ? importedTypes : null,
            importedDirectives.Count > 0 ? importedDirectives : null,
            document.SchemaExtensions, // Keep schema extensions
            document.TypeExtensions // Keep type extensions
        );
    }
}