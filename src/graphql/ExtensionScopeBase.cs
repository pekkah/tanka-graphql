using System.Threading.Tasks;
using tanka.graphql.resolvers;
using tanka.graphql.validation;
using GraphQLParser.AST;

namespace tanka.graphql
{
    public abstract class ExtensionScopeBase : IExtensionScope
    {
        public virtual Task BeginValidationAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task EndValidationAsync(ValidationResult validationResult)
        {
            return Task.CompletedTask;
        }

        public virtual Task EndExecuteAsync(ExecutionResult executionResult)
        {
            return Task.CompletedTask;
        }

        public virtual Task BeginParseDocumentAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task EndParseDocumentAsync(GraphQLDocument document)
        {
            return Task.CompletedTask;
        }

        public virtual Resolver Resolver(Resolver next)
        {
            return next;
        }
    }
}