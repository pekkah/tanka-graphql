using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Tanka.GraphQL.Server
{
    public class RequestServicesScopeExtension : IExecutorExtension
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RequestServicesScopeExtension(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<IExtensionScope> BeginExecuteAsync(ExecutionOptions options)
        {
            IExtensionScope context =
                new ContextExtensionScope<IServiceProvider>(_httpContextAccessor.HttpContext.RequestServices);
            return Task.FromResult(context);
        }
    }
}