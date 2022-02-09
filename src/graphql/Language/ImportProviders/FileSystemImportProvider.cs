using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Language.ImportProviders;

public class FileSystemImportProvider : IImportProvider
{
    public static string FileExtension = ".graphql";
    private readonly string _rootPath;

    public FileSystemImportProvider() : this(AppContext.BaseDirectory)
    {
    }

    public FileSystemImportProvider(string rootPath)
    {
        _rootPath = rootPath;
    }

    public bool CanImport(string path, string[]? types)
    {
        path = GetFullPath(path);
        return File.Exists(path);
    }

    public async ValueTask<TypeSystemDocument> ImportAsync(string path, string[]? types, ParserOptions options)
    {
        path = GetFullPath(path);
        var source = await File.ReadAllTextAsync(path);

        // parse normally
        var document = (TypeSystemDocument)source;

        if (document.Imports is not null)
        {
            var rootPath = Path.GetDirectoryName(path);
            document = document
                .WithImports(document.Imports.Select(import => FullyQualify(import, rootPath ?? _rootPath)).ToList());
        }

        // if no type filter provided import all
        if (types is { Length: > 0 })
        {

            document = document
                .WithDirectiveDefinitions(document.DirectiveDefinitions
                    ?.Where(type => types.Contains(type.Name.ToString())).ToList())
                .WithTypeDefinitions(document.TypeDefinitions
                    ?.Where(type => types.Contains(type.Name.ToString())).ToList())
                .WithTypeExtensions(document.TypeExtensions
                    ?.Where(type => types.Contains(type.Name.ToString())).ToList());
        }

        return document;
    }

    private Import FullyQualify(Import import, string rootPath)
    {
        var from = import.From.ToString();

        if (!Path.IsPathRooted(from)) 
            from = Path.Combine(rootPath, from);

        return new Import(import.Types, new StringValue(Encoding.UTF8.GetBytes(from)));
    }

    private string GetFullPath(string path)
    {
        if (!Path.HasExtension(path)) path += FileExtension;

        if (!Path.IsPathRooted(path))
            path = Path.Combine(_rootPath, path);

        return path;
    }
}