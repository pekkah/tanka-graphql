using System.Threading.Tasks;
using GraphQLParser.AST;
using tanka.graphql.resolvers;
using tanka.graphql.validation;

namespace tanka.graphql
{
    public interface IExtensionScope
    {
        Task BeginValidationAsync();

        Task EndValidationAsync(ValidationResult validationResult);

        Task EndExecuteAsync(ExecutionResult executionResult);

        Task BeginParseDocumentAsync();

        Task EndParseDocumentAsync(GraphQLDocument document);

        Resolver Resolver(Resolver next);
    }
}