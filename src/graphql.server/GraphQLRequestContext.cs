using System;
using Microsoft.AspNetCore.Http.Features;

namespace Tanka.GraphQL.Server;

public record GraphQLRequestContext : QueryContext
{
    private FeatureReferences<FeatureInterfaces> _features;

    public GraphQLRequestContext(IFeatureCollection features) : base(features)
    {
    }

    public GraphQLRequestContext()
    {
    }

    public IServiceProvider RequestServices => ServiceProvidersFeature.RequestServices;

    private IServiceProvidersFeature ServiceProvidersFeature =>
        _features.Fetch(ref _features.Cache.ServiceProviders, _ => null)!;

    private struct FeatureInterfaces
    {
        public IServiceProvidersFeature? ServiceProviders;
    }
}