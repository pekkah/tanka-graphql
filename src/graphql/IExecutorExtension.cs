using System.Threading.Tasks;

namespace Tanka.GraphQL;

public interface IExecutorExtension
{
    //todo: change to ValueTask ?
    Task<IExtensionScope> BeginExecuteAsync(ExecutionOptions options);
}