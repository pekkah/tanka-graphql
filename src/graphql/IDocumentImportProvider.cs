using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQLParser.AST;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL
{
    /// <summary>
    ///     Import provider
    /// </summary>
    public interface IDocumentImportProvider
    {
        bool CanImport(DirectiveInstance import);

        ValueTask<IEnumerable<ASTNode>> ImportAsync(DirectiveInstance import, ParserOptions options);
    }
}