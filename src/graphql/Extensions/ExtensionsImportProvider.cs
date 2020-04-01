using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Tanka.GraphQL.Extensions.Analysis;
using Tanka.GraphQL.Language;

namespace Tanka.GraphQL.Extensions
{
    public class ExtensionsImportProvider : IImportProvider
    {
        private static readonly Regex _match = new Regex(@"tanka:\/\/(?<extension>\w.+)");

        private static readonly Dictionary<string, IEnumerable<object>> _extensions =
            new Dictionary<string, IEnumerable<object>>
            {
                ["cost-analysis"] = CostAnalyzer.CostDirectiveAst
            };

        public bool CanImport(string path, string[] types)
        {
            var match = _match.Match(path);

            if (!match.Success)
                return false;

            var extension = match.Groups["extension"].Value;
            return _extensions.ContainsKey(extension);
        }

        public ValueTask<IEnumerable<object>> ImportAsync(string path, string[] types, ParserOptions options)
        {
            var match = _match.Match(path);

            var extension = match.Groups["extension"].Value;
            return new ValueTask<IEnumerable<object>>(_extensions[extension]);
        }
    }
}