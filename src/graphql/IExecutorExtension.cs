using System.Threading.Tasks;

namespace Tanka.GraphQL;

public interface IExecutorExtension
{
    Task<IExtensionScope> BeginExecuteAsync(ExecutionOptions options);
}