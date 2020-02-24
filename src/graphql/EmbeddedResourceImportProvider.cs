using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GraphQLParser.AST;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL
{
    public class EmbeddedResourceImportProvider : IDocumentImportProvider
    {
        private static readonly Regex _match = new Regex(@"embedded:\/\/(?<assembly>\w.+)\/(?<resourceName>\w.+)");

        public bool CanImport(DirectiveInstance import)
        {
            var path = import.GetArgument<string>("path");
            return _match.IsMatch(path);
        }

        public async ValueTask<IEnumerable<ASTNode>> ImportAsync(DirectiveInstance import, ParserOptions options)
        {
            var path = import.GetArgument<string>("path");
            var matches = _match.Match(path);
            var assembly = matches.Groups["assembly"].Value;
            var resourceName = matches.Groups["resourceName"].Value;

            var source = GetSource(assembly, resourceName);
            var document = await Parser.ParseDocumentAsync(source, options);

            return document.Definitions;
        }

        private string GetSource(string assemblyName, string resourceName)
        {
            Assembly assembly = null;

            if (Assembly.GetExecutingAssembly().FullName.StartsWith($"{assemblyName},"))
                assembly = Assembly.GetExecutingAssembly();

            if (assembly == null)
            {
                assembly = Assembly.Load(assemblyName);
            }

            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}