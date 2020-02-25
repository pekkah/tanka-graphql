using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQLParser.AST;

namespace Tanka.GraphQL.Language
{
    /// <summary>
    ///     Import provider
    /// </summary>
    public interface IImportProvider
    {
        bool CanImport(string path, string[] types);

        ValueTask<IEnumerable<ASTNode>> ImportAsync(string path, string[] types, ParserOptions options);
    }
}