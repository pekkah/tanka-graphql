using System.Threading.Tasks;
using fugu.graphql.validation;

namespace fugu.graphql
{
    public interface IExtension
    {
        Task BeginExecuteAsync(ExecutionOptions options);

        Task BeginValidationAsync();

        Task EndValidationAsync(ValidationResult validationResult);

        Task EndExecuteAsync(ExecutionResult executionResult);
    }
}