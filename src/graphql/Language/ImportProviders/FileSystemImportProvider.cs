using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GraphQLParser.AST;

namespace Tanka.GraphQL.Language.ImportProviders
{
    public class FileSystemImportProvider : IImportProvider
    {
        public static string FileExtension = ".graphql";

        public bool CanImport(string path, string[] types)
        {
            if (!Path.HasExtension(path)) path += FileExtension;

            return File.Exists(path);
        }

        public async ValueTask<IEnumerable<ASTNode>> ImportAsync(string path, string[] types, ParserOptions options)
        {
            if (!Path.HasExtension(path))
                path += FileExtension;

            var source = File.ReadAllText(path);
            var document = await Parser.ParseDocumentAsync(source, options);
            return document.Definitions;
        }
    }
}