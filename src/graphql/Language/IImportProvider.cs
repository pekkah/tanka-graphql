using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Language
{
    /// <summary>
    ///     Import provider
    /// </summary>
    public interface IImportProvider
    {
        bool CanImport(string path, string[]? types);

        ValueTask<TypeSystemDocument> ImportAsync(string path, string[]? types, ParserOptions options);
    }
}