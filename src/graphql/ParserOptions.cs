using System.Collections.Generic;
using Tanka.GraphQL.Extensions;

namespace Tanka.GraphQL
{
    public class ParserOptions
    {
        public List<IDocumentImportProvider> ImportProviders { get; set; } = new List<IDocumentImportProvider>()
        {
            new ExtensionsImportProvider(),
            new FileSystemImportProvider(),
            new EmbeddedResourceImportProvider()
        };
    }
}