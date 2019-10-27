using System.Threading.Tasks;
using GraphQLParser.AST;
using Tanka.GraphQL.Validation;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Server
{
    public class ContextExtensionScope<T> : IExtensionScope
    {
        public ContextExtensionScope(T context)
        {
            Context = context;
        }

        public T Context { get; protected set; }

        public ValueTask BeginValidationAsync()
        {
            return default;
        }

        public ValueTask EndValidationAsync(ValidationResult validationResult)
        {
            return default;
        }

        public ValueTask EndExecuteAsync(IExecutionResult executionResult)
        {
            return default;
        }

        public ValueTask BeginParseDocumentAsync()
        {
            return default;
        }

        public ValueTask EndParseDocumentAsync(GraphQLDocument document)
        {
            return default;
        }

        public Resolver Resolver(Resolver next)
        {
            return next;
        }
    }
}