using System.Linq;
using System.Threading.Tasks;
using Tanka.GraphQL.Language;
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
        ///     with given options
        /// </summary>
        /// <param name="document"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static async Task<TypeSystemDocument> ParseTypeSystemDocumentAsync(string document, ParserOptions? options = null)
        {
            var root = ParseTypeSystemDocument(document);

            if (options != null && root.Imports != null)
            {
                foreach (var import in root.Imports)
                {
                    var from = import.From.ToString();
                    var types = import.Types?.Select(t => t.ToString()).ToArray();
                    
                    var provider = options.ImportProviders
                        .FirstOrDefault(p => p.CanImport(from, types));

                    if (provider == null)
                        throw new DocumentException($"Import from '{from}' failed. No provider capable of import found.");

                    var importedDocument = await provider.ImportAsync(from, types, options);

                    root = root
                        .Merge(importedDocument);
                }
            }

            return root;
        }
    }
}