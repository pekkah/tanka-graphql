using System.Collections.Generic;
using Tanka.GraphQL.Extensions;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.ImportProviders;

namespace Tanka.GraphQL
{
    public class ParserOptions
    {
        public static ParserOptions Sdl = new ParserOptions
        {
            ImportProviders = new List<IImportProvider>
            {
                new ExtensionsImportProvider(),
                new FileSystemImportProvider(),
                new EmbeddedResourceImportProvider()
            }
        };

        public List<IImportProvider> ImportProviders { get; set; }
    }
}