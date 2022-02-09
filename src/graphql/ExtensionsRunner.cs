using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Validation;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL
{
    public class ExtensionsRunner
    {
        private readonly List<IExtensionScope> _scopes = new();
        private readonly Dictionary<Type, IExtensionScope> _scopesDictionary = new();

        public ExtensionsRunner(IReadOnlyList<IExtensionScope> extensions)
        {
            _scopes.AddRange(extensions);
            foreach (var extensionScope in extensions)
                _scopesDictionary.Add(extensionScope.GetType(), extensionScope);
        }

        public T Extension<T>() where T : IExtensionScope
        {
            var extensionScope = Extension(typeof(T));
            return (T)extensionScope;
        }

        public IExtensionScope Extension(Type extensionScopeType)
        {
            if (!_scopesDictionary.TryGetValue(extensionScopeType, out var extensionScope))
                throw new InvalidOperationException($"Could not find extension scope of type {extensionScopeType}");

            return extensionScope;
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

        public async Task EndParseDocumentAsync(ExecutableDocument document)
        {
            foreach (var extension in _scopes) await extension.EndParseDocumentAsync(document);
        }

        public async ValueTask BeginResolveAsync(IResolverContext context)
        {
            foreach (var extension in _scopes)
            {
                var task = extension.BeginResolveAsync(context);

                if (task.IsCompletedSuccessfully)
                    continue;

                await task;
            }
        }

        public async ValueTask EndResolveAsync(IResolverContext context, IResolverResult result)
        {
            foreach (var extension in _scopes)
            {
                var task = extension.EndResolveAsync(context, result);

                if (task.IsCompletedSuccessfully)
                    continue;

                await task;
            }
        }
    }
}