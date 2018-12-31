using System.Threading.Tasks;
using tanka.graphql.resolvers;
using tanka.graphql.validation;
using GraphQLParser.AST;

namespace tanka.graphql
{
    public interface IExtension
    {
        Task BeginExecuteAsync(ExecutionOptions options);

        Task BeginValidationAsync();

        Task EndValidationAsync(ValidationResult validationResult);

        Task EndExecuteAsync(ExecutionResult executionResult);

        Task BeginParseDocumentAsync();

        Task EndParseDocumentAsync(GraphQLDocument document);

        Resolver Resolver(Resolver next);
    }
}