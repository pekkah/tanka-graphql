using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQLParser.AST;
using tanka.graphql.resolvers;
using tanka.graphql.validation;

namespace tanka.graphql
{
    public class Extensions
    {
        private readonly List<IExtension> _extensions = new List<IExtension>();
        private readonly List<IExtensionScope> _scopes = new List<IExtensionScope>();

        public Extensions(IEnumerable<IExtension> extensions)
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
            return Task.WhenAll(_scopes.Select(e => e.BeginValidationAsync()));
        }

        public async Task EndValidationAsync(ValidationResult validationResult)
        {
            foreach (var extension in _scopes) await extension.EndValidationAsync(validationResult);
        }

        public async Task EndExecuteAsync(ExecutionResult executionResult)
        {
            foreach (var extension in _scopes) await extension.EndExecuteAsync(executionResult);
        }

        public Task BeginParseDocumentAsync()
        {
            return Task.WhenAll(_scopes.Select(e => e.BeginParseDocumentAsync()));
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