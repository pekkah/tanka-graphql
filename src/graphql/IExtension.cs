using System.Threading.Tasks;

namespace tanka.graphql
{
    public interface IExtension
    {
        Task<IExtensionScope> BeginExecuteAsync(ExecutionOptions options);
    }
}