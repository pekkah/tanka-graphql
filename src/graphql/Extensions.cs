using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using fugu.graphql.validation;

namespace fugu.graphql
{
    public class Extensions
    {
        private readonly List<IExtension> _extensions = new List<IExtension>();

        public void Use(IExtension extension)
        {
            _extensions.Add(extension);
        }

        public void Use(IEnumerable<IExtension> extensions)
        {
            _extensions.AddRange(extensions);
        }

        public Task BeginExecuteAsync(ExecutionOptions options)
        {
            return Task.WhenAll(_extensions.Select(e => e.BeginExecuteAsync(options)));
        }

        public Task BeginValidationAsync()
        {
            return Task.WhenAll(_extensions.Select(e => e.BeginValidationAsync()));
        }

        public async Task EndValidationAsync(ValidationResult validationResult)
        {
            foreach (var extension in _extensions)
            {
                await extension.EndValidationAsync(validationResult);
            }
        }

        public async Task EndExecuteAsync(ExecutionResult executionResult)
        {
            foreach (var extension in _extensions)
            {
                await extension.EndExecuteAsync(executionResult);
            }
        }
    }
}