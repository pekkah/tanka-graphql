using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GraphQLParser.AST;
using Tanka.GraphQL.Extensions.Analysis;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Extensions
{
    public class ExtensionsImportProvider : IDocumentImportProvider
    {
        private static readonly Regex _match = new Regex(@"tanka:\/\/(?<extension>\w.+)");
        private static readonly Dictionary<string, IEnumerable<ASTNode>> _extensions = new Dictionary<string, IEnumerable<ASTNode>>()
        {
            ["cost-analysis"] = CostAnalyzer.CostDirectiveAst
        };

        public bool CanImport(DirectiveInstance import)
        {
            var path = import.GetArgument<string>("path");
            var match = _match.Match(path);

            if (!match.Success)
                return false;

            var extension = match.Groups["extension"].Value;
            return _extensions.ContainsKey(extension);
        }

        public ValueTask<IEnumerable<ASTNode>> ImportAsync(DirectiveInstance import, ParserOptions options)
        {
            var path = import.GetArgument<string>("path");
            var match = _match.Match(path);

            var extension = match.Groups["extension"].Value;
            return new ValueTask<IEnumerable<ASTNode>>(_extensions[extension]);
        }
    }
}