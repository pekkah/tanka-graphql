using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GraphQLParser.AST;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL
{
    public class FileSystemImportProvider : IDocumentImportProvider
    {
        public static string FileExtension = ".graphql";

        public bool CanImport(DirectiveInstance import)
        {
            var path = import.GetArgument<string>("path");

            if (!Path.HasExtension(path))
            {
                path += FileExtension;
            }

            return File.Exists(path);
        }

        public async ValueTask<IEnumerable<ASTNode>> ImportAsync(DirectiveInstance import, ParserOptions options)
        {
            var path = import.GetArgument<string>("path");

            if (!Path.HasExtension(path))
                path += FileExtension;

            var source = File.ReadAllText(path);
            var document = await Parser.ParseDocumentAsync(source, options);
            return document.Definitions;
        }
    }
}