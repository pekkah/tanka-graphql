using System.Threading.Tasks;

namespace Tanka.GraphQL.Extensions.Tracing;

public class TraceExtension : IExecutorExtension
{
    public Task<IExtensionScope> BeginExecuteAsync(ExecutionOptions options)
    {
        IExtensionScope scope = new TraceExtensionScope(options);
        return Task.FromResult(scope);
    }
}