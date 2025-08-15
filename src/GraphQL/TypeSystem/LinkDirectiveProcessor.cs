using System.Text.Json;

using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.ValueSerialization;

namespace Tanka.GraphQL.TypeSystem;

/// <summary>
/// Information about a link directive
/// </summary>
public record LinkInfo(
    string Url,
    string? As = null,
    IReadOnlyList<string>? Imports = null,
    string? Purpose = null);

/// <summary>
/// Provides information about existing types and directives for import filtering
/// </summary>
public interface IExistingTypesProvider
{
    bool ContainsType(string typeName);
    bool ContainsDirective(string directiveName);
}

/// <summary>
/// Processes @link directives on schema definitions to resolve schema imports and linking
/// </summary>
public static class LinkDirectiveProcessor
{
    /// <summary>
    /// Process @link directives on schema definitions and schema extensions
    /// </summary>
    public static IEnumerable<LinkInfo> ProcessLinkDirectives(
        IEnumerable<SchemaDefinition>? schemaDefinitions,
        IEnumerable<SchemaExtension>? schemaExtensions)
    {
        // Process schema definitions
        if (schemaDefinitions != null)
        {
            foreach (var schema in schemaDefinitions)
            {
                if (schema.Directives != null)
                {
                    foreach (var link in ProcessDirectives(schema.Directives))
                        yield return link;
                }
            }
        }

        // Process schema extensions  
        if (schemaExtensions != null)
        {
            foreach (var extension in schemaExtensions)
            {
                if (extension.Directives != null)
                {
                    foreach (var link in ProcessDirectives(extension.Directives))
                        yield return link;
                }
            }
        }
    }

    private static IEnumerable<LinkInfo> ProcessDirectives(IReadOnlyList<Directive> directives)
    {
        foreach (var directive in directives)
        {
            if (directive.Name.Value == "link")
            {
                yield return ProcessLinkDirective(directive);
            }
        }
    }

    private static LinkInfo ProcessLinkDirective(Directive linkDirective)
    {
        string? url = null;
        string? @as = null;
        List<string>? importList = null;
        string? purpose = null;

        // Parse directive arguments
        if (linkDirective.Arguments != null)
        {
            foreach (var argument in linkDirective.Arguments)
            {
                switch (argument.Name.Value)
                {
                    case "url":
                        if (argument.Value is StringValue urlValue)
                            url = urlValue.ToString();
                        break;
                    case "as":
                        if (argument.Value is StringValue asValue)
                            @as = asValue.ToString();
                        break;
                    case "import":
                        importList = ParseImportArgument(argument.Value);
                        break;
                    case "for":
                        if (argument.Value is EnumValue purposeValue)
                            purpose = purposeValue.Name.Value;
                        break;
                }
            }
        }

        if (url == null)
            throw new InvalidOperationException("@link directive requires 'url' argument");

        return new LinkInfo(url, @as, importList, purpose);
    }

    private static List<string>? ParseImportArgument(ValueBase importValue)
    {
        return importValue switch
        {
            ListValue listValue => ParseImportList(listValue),
            _ => null
        };
    }

    private static List<string> ParseImportList(ListValue listValue)
    {
        var imports = new List<string>();
        var converter = new LinkImportScalarConverter();

        foreach (var item in listValue)
        {
            var parsed = converter.ParseLiteral(item);
            switch (parsed)
            {
                case string simpleName:
                    imports.Add(simpleName);
                    break;
                case LinkImport linkImport:
                    // For now, use the name directly - alias handling can be added later
                    imports.Add(linkImport.Name);
                    break;
            }
        }

        return imports;
    }

    /// <summary>
    /// Resolve all linked schemas and their dependencies, returning a merged document
    /// </summary>
    public static async Task<TypeSystemDocument?> ResolveLinkedSchemasAsync(
        IEnumerable<LinkInfo> linkInfos,
        ISchemaLoader schemaLoader,
        IExistingTypesProvider existingTypesProvider,
        int maxDepth = 10)
    {
        var processedUrls = new HashSet<string>();
        var allResolvedSchemas = new List<(TypeSystemDocument doc, LinkInfo link)>();

        // Process each link info
        foreach (var linkInfo in linkInfos)
        {
            if (processedUrls.Add(linkInfo.Url))
            {
                var resolvedSchema = await LoadAndResolveSchemaAsync(
                    linkInfo,
                    schemaLoader,
                    processedUrls,
                    maxDepth,
                    0);

                if (resolvedSchema != null)
                {
                    allResolvedSchemas.Add((resolvedSchema, linkInfo));
                }
            }
        }

        // If no schemas were resolved, return null
        if (allResolvedSchemas.Count == 0)
            return null;

        // Apply import filtering and merge all schemas
        return MergeFilteredSchemas(allResolvedSchemas, existingTypesProvider);
    }

    /// <summary>
    /// Apply import filtering to a schema document based on LinkInfo
    /// </summary>
    public static TypeSystemDocument ApplyImportFiltering(
        TypeSystemDocument document,
        LinkInfo linkInfo,
        IExistingTypesProvider existingTypesProvider)
    {
        // If no imports specified, return everything except types/directives that already exist
        if (linkInfo.Imports == null || linkInfo.Imports.Count == 0)
        {
            // Filter out types that already exist to avoid duplicates
            var filteredTypes = document.TypeDefinitions?.Where(t => !existingTypesProvider.ContainsType(t.Name.Value)).ToList();
            var filteredDirectives = document.DirectiveDefinitions?.Where(d => !existingTypesProvider.ContainsDirective(d.Name.Value)).ToList();

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
                if (directive != null && !existingTypesProvider.ContainsDirective(directiveName))
                {
                    importedDirectives.Add(directive);
                }
            }
            else
            {
                // Import type
                var type = document.TypeDefinitions?.FirstOrDefault(t => t.Name.Value == import);
                if (type != null && !existingTypesProvider.ContainsType(import))
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

    /// <summary>
    /// Load and resolve a schema with its dependencies (depth-first)
    /// </summary>
    private static async Task<TypeSystemDocument?> LoadAndResolveSchemaAsync(
        LinkInfo linkInfo,
        ISchemaLoader schemaLoader,
        HashSet<string> visitedUrls,
        int maxDepth,
        int currentDepth)
    {
        if (currentDepth >= maxDepth)
            throw new InvalidOperationException($"Maximum link depth exceeded for: {linkInfo.Url}");

        // Load the raw schema
        var schema = await schemaLoader.LoadSchemaAsync(linkInfo.Url);
        if (schema == null)
            return null;

        // Extract its @link directives
        var nestedLinkInfos = ProcessLinkDirectives(
            schema.SchemaDefinitions,
            schema.SchemaExtensions);

        // Process each linked schema FIRST (depth-first)
        var linkedSchemas = new List<(LinkInfo info, TypeSystemDocument doc)>();
        foreach (var nestedLinkInfo in nestedLinkInfos)
        {
            if (visitedUrls.Add(nestedLinkInfo.Url))
            {
                var linkedDoc = await LoadAndResolveSchemaAsync(
                    nestedLinkInfo,
                    schemaLoader,
                    visitedUrls,
                    maxDepth,
                    currentDepth + 1);

                if (linkedDoc != null)
                    linkedSchemas.Add((nestedLinkInfo, linkedDoc));
            }
        }

        // Build merged document: dependencies first, then current schema
        TypeSystemDocument? mergedDocument = null;

        // Add filtered linked schemas
        foreach (var (nestedLinkInfo, linkedDoc) in linkedSchemas)
        {
            // For nested schemas, we use a simple existing types provider that doesn't filter
            var simpleProvider = new SimpleExistingTypesProvider();
            var filtered = ApplyImportFiltering(linkedDoc, nestedLinkInfo, simpleProvider);
            if (mergedDocument == null)
                mergedDocument = filtered;
            else
                mergedDocument = mergedDocument.WithTypeSystem(filtered);
        }

        // Apply import filtering to the current schema based on original linkInfo
        var simpleProviderForCurrent = new SimpleExistingTypesProvider();
        var currentFiltered = ApplyImportFiltering(schema, linkInfo, simpleProviderForCurrent);

        // Add current schema last (so it can reference linked types)
        if (mergedDocument == null)
            mergedDocument = currentFiltered;
        else
            mergedDocument = mergedDocument.WithTypeSystem(currentFiltered);

        return mergedDocument;
    }

    /// <summary>
    /// Merge multiple filtered schemas into a single document
    /// </summary>
    private static TypeSystemDocument MergeFilteredSchemas(
        IReadOnlyList<(TypeSystemDocument doc, LinkInfo link)> schemas,
        IExistingTypesProvider existingTypesProvider)
    {
        TypeSystemDocument? mergedDocument = null;

        foreach (var (doc, link) in schemas)
        {
            var filtered = ApplyImportFiltering(doc, link, existingTypesProvider);

            if (mergedDocument == null)
                mergedDocument = filtered;
            else
                mergedDocument = mergedDocument.WithTypeSystem(filtered);
        }

        return mergedDocument ?? new TypeSystemDocument(null, null, null, null, null);
    }
}

/// <summary>
/// Simple implementation of IExistingTypesProvider that doesn't filter anything
/// Used for nested dependency resolution where we want to include all types
/// </summary>
internal class SimpleExistingTypesProvider : IExistingTypesProvider
{
    public bool ContainsType(string typeName) => false;
    public bool ContainsDirective(string directiveName) => false;
}