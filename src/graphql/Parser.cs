using System;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL
{
    public static class Parser
    {
        /// <summary>
        ///     Parse <see cref="document" /> into <see cref="ExecutableDocument" />
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public static ExecutableDocument ParseDocument(string document)
        {
            var parser = Language.Parser.Create(document);
            return parser.ParseExecutableDocument();
        }

        /// <summary>
        ///     Parse <see cref="document" /> into <see cref="TypeSystemDocument" />
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public static TypeSystemDocument ParseTypeSystemDocument(string document)
        {
            var parser = Language.Parser.Create(document);
            return parser.ParseTypeSystemDocument();
        }

        /// <summary>
        ///     Parse <see cref="document" /> into <see cref="TypeSystemDocument" />
        ///     with default options
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        [Obsolete]
        public static async Task<TypeSystemDocument> ParseTypeSystemDocumentAsync(string document, ParserOptions? options = null)
        {
            await Task.Yield();
            // ReSharper disable once MethodHasAsyncOverload
            return ParseTypeSystemDocument(document);
        }
    }
}