using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQLParser.AST;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.Validation;

namespace Tanka.GraphQL
{
    public class ExtensionsRunner
    {
        private readonly List<IExecutorExtension> _extensions = new List<IExecutorExtension>();
        private readonly List<IExtensionScope> _scopes = new List<IExtensionScope>();

        public ExtensionsRunner(IEnumerable<IExecutorExtension> extensions)
        {
            _extensions.AddRange(extensions.Reverse());
        }

        public async Task BeginExecuteAsync(ExecutionOptions options)
        {
            var scopes = await Task.WhenAll(_extensions.Select(e => e.BeginExecuteAsync(options)));
            _scopes.AddRange(scopes);
        }

        public Task BeginValidationAsync()
        {
            return Task.WhenAll(_scopes.Select(e => e.BeginValidationAsync().AsTask()));
        }

        public async Task EndValidationAsync(ValidationResult validationResult)
        {
            foreach (var extension in _scopes) await extension.EndValidationAsync(validationResult);
        }

        public async Task EndExecuteAsync(IExecutionResult executionResult)
        {
            foreach (var extension in _scopes) 
                await extension.EndExecuteAsync(executionResult);
        }

        public Task BeginParseDocumentAsync()
        {
            return Task.WhenAll(_scopes.Select(e => e.BeginParseDocumentAsync().AsTask()));
        }

        public async Task EndParseDocumentAsync(GraphQLDocument document)
        {
            foreach (var extension in _scopes) await extension.EndParseDocumentAsync(document);
        }

        public Resolver Resolver(ResolverContext resolverContext, Resolver fieldResolver)
        {
            var result = fieldResolver;
            foreach (var extension in _scopes) result = extension.Resolver(result);

            return result;
        }
    }
}