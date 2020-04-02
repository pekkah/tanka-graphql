using System;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Validation;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Server
{
    public class ContextExtensionScope<T> : IExtensionScope
    {
        private readonly bool _dispose;

        public ContextExtensionScope(T context, bool dispose = true)
        {
            _dispose = dispose;
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
            if (Context is IDisposable disposable && _dispose)
                disposable.Dispose();

            return default;
        }

        public ValueTask BeginParseDocumentAsync()
        {
            return default;
        }

        public ValueTask EndParseDocumentAsync(ExecutableDocument document)
        {
            return default;
        }

        public ValueTask BeginResolveAsync(IResolverContext context)
        {
            return default;
        }

        public ValueTask EndResolveAsync(IResolverContext context, IResolverResult result)
        {
            return default;
        }
    }
}