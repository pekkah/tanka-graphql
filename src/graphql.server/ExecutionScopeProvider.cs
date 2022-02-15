using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Tanka.GraphQL.Server;

public class ExecutionScopeProvider : IExecutorExtension
{
    private readonly IServiceProvider _serviceProvider;

    public ExecutionScopeProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<IExtensionScope> BeginExecuteAsync(ExecutionOptions options)
    {
        var scope = _serviceProvider.CreateScope();
        IExtensionScope context = new ContextExtensionScope<IServiceScope>(
            scope);

        return Task.FromResult(context);
    }
}