using System.Threading.Tasks;

namespace tanka.graphql.tracing
{
    public class TraceExtension : IExtension
    {
        public Task<IExtensionScope> BeginExecuteAsync(ExecutionOptions options)
        {
            IExtensionScope scope = new TraceExtensionScope(options);
            return Task.FromResult(scope);
        }
    }
}