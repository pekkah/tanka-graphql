using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes.TypeSystem;


namespace Tanka.GraphQL.Language.ImportProviders
{
    public class FileSystemImportProvider : IImportProvider
    {
        public static string FileExtension = ".graphql";
        private readonly string _rootPath;

        public FileSystemImportProvider() : this(Environment.CurrentDirectory)
        {
        }

        public FileSystemImportProvider(string rootPath)
        {
            _rootPath = rootPath;
        }

        public bool CanImport(string path, string[] types)
        {
            path = GetFullPath(path);
            return File.Exists(path);
        }

        public async ValueTask<TypeSystemDocument> ImportAsync(string path, string[]? types, ParserOptions options)
        {
            path = GetFullPath(path);
            var source = File.ReadAllText(path);

            // we need new options with correctly rooted file system import provider
            var rootPath = Path.GetDirectoryName(path);
            var newOptions = options
                .ReplaceImportProvider(
                    this,
                    new FileSystemImportProvider(rootPath));

            // parse normally
            var document = await GraphQL.Parser.ParseTypeSystemDocumentAsync(source, newOptions);

            // if no type filter provided import all
            if (types == null || types.Length == 0)
            {
                return document;
            }

            return document
                .WithDirectiveDefinitions(document.DirectiveDefinitions
                    ?.Where(type => types.Contains<string>(type.Name.ToString())).ToList())
                .WithTypeDefinitions(document.TypeDefinitions
                    ?.Where(type => types.Contains<string>(type.Name.ToString())).ToList())
                .WithTypeExtensions(document.TypeExtensions
                    ?.Where(type => types.Contains<string>(type.Name.ToString())).ToList());
            
        }

        private string GetFullPath(string path)
        {
            if (!Path.HasExtension(path)) path += FileExtension;
            
            if (!Path.IsPathRooted(path)) 
                path = Path.Combine(_rootPath, path);

            return path;
        }
    }
}