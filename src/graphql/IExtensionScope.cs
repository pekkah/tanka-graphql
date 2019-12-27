using System.Threading.Tasks;
using GraphQLParser.AST;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.Validation;

namespace Tanka.GraphQL
{
    public interface IExtensionScope
    {
        ValueTask BeginValidationAsync();

        ValueTask EndValidationAsync(ValidationResult validationResult);

        ValueTask EndExecuteAsync(IExecutionResult executionResult);

        ValueTask BeginParseDocumentAsync();

        ValueTask EndParseDocumentAsync(GraphQLDocument document);

        ValueTask BeginResolveAsync(IResolverContext context);

        ValueTask EndResolveAsync(IResolverContext context, IResolverResult result);
    }
}