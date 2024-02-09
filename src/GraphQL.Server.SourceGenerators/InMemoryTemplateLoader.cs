using System.Collections.Generic;
using System.Threading.Tasks;

using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;

namespace Tanka.GraphQL.Server.SourceGenerators;

internal class InMemoryTemplateLoader(Dictionary<string, string> dictionary) : ITemplateLoader
{
    public Dictionary<string, string> Dictionary { get; } = dictionary;

    public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
    {
        return templateName;
    }

    public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
    {
        return Dictionary[templatePath];
    }

    public ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
    {
        return new ValueTask<string>(Load(context, callerSpan, templatePath));
    }
}