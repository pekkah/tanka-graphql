using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;


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

        public async ValueTask<IEnumerable<object>> ImportAsync(string path, string[] types, ParserOptions options)
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
            return document.TypeDefinitions;
        }

        private string GetFullPath(string path)
        {
            if (!Path.HasExtension(path)) path += FileExtension;

            if (!Path.IsPathRooted(path)) path = Path.Combine(_rootPath, path);

            return path;
        }
    }
}