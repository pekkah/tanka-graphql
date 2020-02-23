using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GraphQLParser;
using GraphQLParser.AST;
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

        public static async Task<GraphQLDocument> ParseDocumentAsync(string document, ParserOptions options)
        {
            var imports = ParseImports(document);

            await Task.Yield();
            return document;
        }

        private static IEnumerable<DirectiveInstance> ParseImports(string document)
        {
            var reader = new StringReader(document);

            while (true)
            {
                var line = reader.ReadLine();

                if (line == null)
                    break;

                if (line.StartsWith("# @import"))
                    yield return ReadImport(line);
                else if (line.StartsWith("#"))
                    continue;
                else
                    break;
            }
        }

        private static DirectiveInstance ReadImport(string commentLineWithImport)
        {
            return null;
        }
    }

    public class ParserOptions
    {
        public List<IDocumentImportProvider> ImportProviders { get; set; }
    }

    public interface IDocumentImportProvider
    {
        bool CanImport(DirectiveInstance import);
    }
}