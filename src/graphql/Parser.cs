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
        ///     with default options
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        [Obsolete]
        public static Task<GraphQLDocument> ParseDocumentAsync(string document)
        {
            return ParseDocumentAsync(document, new ParserOptions());
        }

        /// <summary>
        ///     Parse <see cref="document" /> into <see cref="GraphQLDocument" />
        /// </summary>
        /// <param name="document"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static async Task<GraphQLDocument> ParseDocumentAsync(string document, ParserOptions options)
        {
            var root = ParseDocument(document);
            if (options.ImportProviders != null && options.ImportProviders.Any())
            {
                var imports = ParseImports(document);

                var importedTypeDefs = new List<ASTNode>();
                foreach (var import in imports)
                {
                    var typeDefs = await ImportAsync(import.Path, import.Types, options);
                    importedTypeDefs.AddRange(typeDefs);
                }

                if (root.Definitions == null)
                    root.Definitions = new List<ASTNode>();

                root.Definitions.AddRange(importedTypeDefs);
                return root;
            }

            return root;
        }

        private static ValueTask<IEnumerable<ASTNode>> ImportAsync(string path, string[] types, ParserOptions options)
        {
            var provider = options.ImportProviders
                .FirstOrDefault(p => p.CanImport(path, types));

            if (provider == null)
                throw new InvalidOperationException($"Could not find import provider for '{path}'");

            return provider.ImportAsync(path, types, options);
        }

        private static IEnumerable<(string Path, string[] Types)> ParseImports(string document)
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
                else if (line == string.Empty)
                    continue;
                else
                    break;
            }
        }

        private static (string Path, string[] Types) ReadImport(string commentLineWithImport)
        {
            var lineWithImport = commentLineWithImport.Substring(1).Trim();

            // hack to parse directive without a type
            var document = ParseDocument($"scalar Hack {lineWithImport}");
          
            // find import
            var import = document.Definitions
                .OfType<GraphQLScalarTypeDefinition>()
                .Single()
                .Directives
                .Single();

            // parse args
            var pathArg = import.Arguments.Single(a => a.Name.Value == "path");
            var typesArg = import.Arguments.SingleOrDefault(a => a.Name.Value == "types");

            var path = Values.CoerceValue(
                _ => Enumerable.Empty<KeyValuePair<string, InputObjectField>>(),
                ScalarType.GetStandardConverter,
                pathArg.Value,
                ScalarType.NonNullString)
                .ToString();

            // types can be null
            if (typesArg != null)
            {
                var typesObjects = (IEnumerable<object>)Values.CoerceValue(
                    _ => Enumerable.Empty<KeyValuePair<string, InputObjectField>>(),
                    ScalarType.GetStandardConverter,
                    typesArg.Value,
                    new List(ScalarType.NonNullString));

                var types = typesObjects.Select(o => o.ToString())
                    .ToArray();

                return (path, types);
            }

            // no types given
            return (path, null);
        }
    }
}