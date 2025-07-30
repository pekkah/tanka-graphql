using System;

using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;

using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Server;

public static class ResolverContextExtensions
{
    public static T GetRequiredService<T>(this ResolverContextBase context) where T : notnull
    {
        var serviceProviderFeature = context.QueryContext.Features.Get<IRequestServicesFeature>();

        if (serviceProviderFeature is null)
            throw new InvalidOperationException($"{nameof(IRequestServicesFeature)} is not set in the query context");

        return serviceProviderFeature.RequestServices.GetRequiredService<T>();
    }
}