using System.Threading.Tasks;
using GraphQLParser.AST;
using tanka.graphql.resolvers;
using tanka.graphql.validation;

namespace tanka.graphql
{
    public interface IExtensionScope
    {
        ValueTask BeginValidationAsync();

        ValueTask EndValidationAsync(ValidationResult validationResult);

        ValueTask EndExecuteAsync(IExecutionResult executionResult);

        ValueTask BeginParseDocumentAsync();

        ValueTask EndParseDocumentAsync(GraphQLDocument document);

        Resolver Resolver(Resolver next);
    }
}