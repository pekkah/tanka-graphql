using System.Threading.Tasks;
using fugu.graphql.validation;
using GraphQLParser.AST;

namespace fugu.graphql
{
    public interface IExtension
    {
        Task BeginExecuteAsync(ExecutionOptions options);

        Task BeginValidationAsync();

        Task EndValidationAsync(ValidationResult validationResult);

        Task EndExecuteAsync(ExecutionResult executionResult);

        Task BeginParseDocumentAsync();

        Task EndParseDocumentAsync(GraphQLDocument document);
    }
}