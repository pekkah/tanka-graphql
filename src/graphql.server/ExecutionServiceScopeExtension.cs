using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Tanka.GraphQL.Server
{
    public class ExecutionServiceScopeExtension : IExecutorExtension
    {
        private readonly IServiceProvider _services;

        public ExecutionServiceScopeExtension(IServiceProvider services)
        {
            _services = services;
        }
        public Task<IExtensionScope> BeginExecuteAsync(ExecutionOptions options)
        {
            IExtensionScope context = new ContextExtensionScope<IServiceScope>(_services.CreateScope());
            return Task.FromResult(context);
        }
    }
}