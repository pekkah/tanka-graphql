using System.Threading.Tasks;

namespace Tanka.GraphQL
{
    public interface IExtensionsRunnerFactory
    {
        Task<ExtensionsRunner> BeginScope(ExecutionOptions options);
    }
}