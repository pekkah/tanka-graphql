using System.Collections.Generic;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.ImportProviders;
//using Tanka.GraphQL.Extensions;

namespace Tanka.GraphQL;

public class ParserOptions
{
    public static ParserOptions Sdl = new()
    {
        ImportProviders = new List<IImportProvider>
        {
            //new ExtensionsImportProvider(),
            new FileSystemImportProvider(),
            new EmbeddedResourceImportProvider()
        }
    };

    public List<IImportProvider> ImportProviders { get; set; } = new();

    public ParserOptions ReplaceImportProvider(
        IImportProvider provider,
        IImportProvider with)
    {
        var options = new ParserOptions();
        options.ImportProviders.AddRange(ImportProviders);
        var index = options.ImportProviders.IndexOf(provider);
        options.ImportProviders.Remove(provider);
        options.ImportProviders.Insert(index, with);

        return options;
    }
}