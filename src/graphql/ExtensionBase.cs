using System.Threading.Tasks;
using fugu.graphql.resolvers;
using fugu.graphql.validation;
using GraphQLParser.AST;

namespace fugu.graphql
{
    public abstract class ExtensionBase : IExtension
    {
        public virtual Task BeginExecuteAsync(ExecutionOptions options)
        {
            return Task.CompletedTask;
        }

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