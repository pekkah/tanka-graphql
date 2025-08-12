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
        var links = new List<LinkInfo>();

        // Process schema definitions
        if (schemaDefinitions != null)
        {
            foreach (var schema in schemaDefinitions)
            {
                if (schema.Directives != null)
                {
                    links.AddRange(ProcessDirectives(schema.Directives));
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
                    links.AddRange(ProcessDirectives(extension.Directives));
                }
            }
        }

        return links;
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
}