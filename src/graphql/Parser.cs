using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GraphQLParser;
using GraphQLParser.AST;
using Tanka.GraphQL.Execution;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.SDL;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL
{
    public static class Parser
    {
        /// <summary>
        ///     Parse <see cref="document" /> into <see cref="GraphQLDocument" />
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public static GraphQLDocument ParseDocument(string document)
        {
            var lexer = new Lexer();
            var parser = new GraphQLParser.Parser(lexer);
            return parser.Parse(new Source(document));
        }

        /// <summary>
        ///     Parse <see cref="document" /> into <see cref="GraphQLDocument" />
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public static Task<GraphQLDocument> ParseDocumentAsync(string document)
        {
            return Task.Factory.StartNew(() =>
            {
                var lexer = new Lexer();
                var parser = new GraphQLParser.Parser(lexer);
                return parser.Parse(new Source(document));
            });
        }

        public static async Task<GraphQLDocument> ParseDocumentAsync(string source, ParserOptions options)
        {
            var root = ParseDocument(source);
            if (options.ImportProviders != null && options.ImportProviders.Any())
            {
                var imports = ParseImports(source);

                var importedTypeDefs = new List<ASTNode>();
                foreach (var import in imports)
                {
                    var typeDefs = await ImportAsync(import, options);
                    importedTypeDefs.AddRange(typeDefs);
                }

                root.Definitions = root.Definitions.Concat(importedTypeDefs);
                return root;
            }

            return root;
        }

        private static ValueTask<IEnumerable<ASTNode>> ImportAsync(DirectiveInstance import, ParserOptions options)
        {
            var provider = options.ImportProviders
                .FirstOrDefault(p => p.CanImport(import));

            if (provider == null)
                throw new InvalidOperationException($"Could not find import provider for '{import}'");

            return provider.ImportAsync(import, options);
        }

        private static IEnumerable<DirectiveInstance> ParseImports(string document)
        {
            var reader = new StringReader(document);

            while (true)
            {
                var line = reader.ReadLine();

                if (line == null)
                    break;

                line = line.Trim();

                if (line.StartsWith("# @import"))
                    yield return ReadImport(line);
                else if (line.StartsWith("#"))
                    continue;
                else if(line == string.Empty)
                    continue;
                else
                    break;
            }
        }

        private static DirectiveInstance ReadImport(string commentLineWithImport)
        {
            var lineWithImport = commentLineWithImport.Substring(1).Trim();

            // hack to parse directive without a type
            var document = ParseDocument($"scalar Hack {lineWithImport}");
            var builder = new SchemaBuilder()
                .Include(ImportDirectiveType)
                .Sdl(document);

            var import = builder.GetTypes<ScalarType>()
                .Single(s => s.Name == "Hack")
                .GetDirective("import");

            return import;
        }

        public static DirectiveType ImportDirectiveType = new DirectiveType(
            "import", 
            new []{DirectiveLocation.SCHEMA}, 
            new Args()
            {
                ["path"] = new Argument(ScalarType.NonNullString, null, "Path"),
                ["types"] = new Argument(new List(ScalarType.NonNullString), null, "Types to import")
            });
    }

    public class ParserOptions
    {
        public List<IDocumentImportProvider> ImportProviders { get; set; } = new List<IDocumentImportProvider>()
        {
            new FileSystemImportProvider(),
            new EmbeddedResourceImportProvider()
        };
    }

    public interface IDocumentImportProvider
    {
        bool CanImport(DirectiveInstance import);

        ValueTask<IEnumerable<ASTNode>> ImportAsync(DirectiveInstance import, ParserOptions options);
    }
}