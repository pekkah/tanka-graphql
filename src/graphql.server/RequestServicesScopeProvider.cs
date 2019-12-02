using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Tanka.GraphQL.Server
{
    public class RequestServicesScopeProvider : IExecutorExtension
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RequestServicesScopeProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<IExtensionScope> BeginExecuteAsync(ExecutionOptions options)
        {
            var serviceProvider = _httpContextAccessor.HttpContext.RequestServices;
            IExtensionScope context = new ContextExtensionScope<IServiceProvider>(
                serviceProvider, 
                false);

            return Task.FromResult(context);
        }
    }
}