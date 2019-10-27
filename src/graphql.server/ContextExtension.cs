using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Tanka.GraphQL.Server
{
    public class ContextExtension<T> : IExecutorExtension
    {
        private readonly IServiceProvider _services;

        public ContextExtension(IServiceProvider services)
        {
            _services = services;
        }

        public Task<IExtensionScope> BeginExecuteAsync(ExecutionOptions options)
        {
            var context = _services.GetRequiredService<T>();
            IExtensionScope scope = new ContextExtensionScope<T>(context);
            return Task.FromResult(scope);
        }
    }
}