using System;
using System.Threading.Tasks;

namespace Tanka.GraphQL.Server
{
    public class ServiceProviderScopeExtension : IExecutorExtension
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceProviderScopeExtension(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task<IExtensionScope> BeginExecuteAsync(ExecutionOptions options)
        {
            IExtensionScope context =
                new ContextExtensionScope<IServiceProvider>(_serviceProvider);
            return Task.FromResult(context);
        }
    }
}