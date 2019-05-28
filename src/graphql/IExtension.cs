using System.Threading.Tasks;

namespace tanka.graphql
{
    public interface IExtension
    {
        //todo: change to ValueTask ?
        Task<IExtensionScope> BeginExecuteAsync(ExecutionOptions options);
    }
}