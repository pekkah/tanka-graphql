using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Tanka.GraphQL.Language.ImportProviders
{
    public class EmbeddedResourceImportProvider : IImportProvider
    {
        private static readonly Regex _match = new Regex(@"embedded:\/\/(?<assembly>\w.+)\/(?<resourceName>\w.+)");

        public bool CanImport(string path, string[] types)
        {
            return _match.IsMatch(path);
        }

        public async ValueTask<IEnumerable<object>> ImportAsync(string path, string[] types, ParserOptions options)
        {
            var matches = _match.Match(path);
            var assembly = matches.Groups["assembly"].Value;
            var resourceName = matches.Groups["resourceName"].Value;

            var source = GetSource(assembly, resourceName);
            var document = await GraphQL.Parser.ParseTypeSystemDocumentAsync(source, options);

            return document.TypeDefinitions;
        }

        private string GetSource(string assemblyName, string resourceName)
        {
            Assembly assembly = null;

            if (Assembly.GetExecutingAssembly().FullName.StartsWith($"{assemblyName},"))
                assembly = Assembly.GetExecutingAssembly();

            if (assembly == null) assembly = Assembly.Load(assemblyName);

            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}