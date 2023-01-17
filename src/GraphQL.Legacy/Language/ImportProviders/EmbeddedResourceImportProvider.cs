using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Language.ImportProviders;

public class EmbeddedResourceImportProvider : IImportProvider
{
    private static readonly Regex _match = new(@"embedded:\/\/(?<assembly>\w.+)\/(?<resourceName>\w.+)");

    public bool CanImport(string path, string[]? types)
    {
        return _match.IsMatch(path);
    }

    public async ValueTask<TypeSystemDocument> ImportAsync(string path, string[]? types, ParserOptions options)
    {
        var matches = _match.Match(path);
        var assembly = matches.Groups["assembly"].Value;
        var resourceName = matches.Groups["resourceName"].Value;

        var source = GetSource(assembly, resourceName);
        var document = (TypeSystemDocument)source;

        /* resource imports are fully qualified */

        if (types is { Length: > 0 })
            document = document
                .WithDirectiveDefinitions(document.DirectiveDefinitions
                    ?.Where(type => types.Contains(type.Name.ToString())).ToList())
                .WithTypeDefinitions(document.TypeDefinitions
                    ?.Where(type => types.Contains(type.Name.ToString())).ToList())
                .WithTypeExtensions(document.TypeExtensions
                    ?.Where(type => types.Contains(type.Name.ToString())).ToList());

        return document;
    }

    private string GetSource(string assemblyName, string resourceName)
    {
        Assembly? assembly = null;

        if (Assembly.GetExecutingAssembly().FullName.StartsWith($"{assemblyName},"))
            assembly = Assembly.GetExecutingAssembly();

        if (assembly == null) assembly = Assembly.Load(assemblyName);

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            var resourceNames = string.Join(",", assembly.GetManifestResourceNames());
            throw new InvalidOperationException(
                $"Could not load manifest stream from '{assemblyName}' with name '{resourceName}'. Found resources: {resourceNames}");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}