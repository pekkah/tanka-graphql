using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tanka.GraphQL
{
    public class ExtensionsRunnerFactory
    {
        public async Task<ExtensionsRunner> BeginScope(ExecutionOptions options)
        {
            var extensions = options.Extensions;
            var scopes = new List<IExtensionScope>();
            foreach (var extension in extensions)
            {
                scopes.Add(await extension.BeginExecuteAsync(options));
            }

            return new ExtensionsRunner(scopes);
        }
    }
}