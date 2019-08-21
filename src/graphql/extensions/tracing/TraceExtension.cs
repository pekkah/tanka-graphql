using System.Threading.Tasks;

namespace tanka.graphql.extensions.tracing
{
    public class TraceExtension : IExecutorExtension
    {
        public Task<IExtensionScope> BeginExecuteAsync(ExecutionOptions options)
        {
            IExtensionScope scope = new TraceExtensionScope(options);
            return Task.FromResult(scope);
        }
    }
}