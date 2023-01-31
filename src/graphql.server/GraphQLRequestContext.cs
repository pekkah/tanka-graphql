using System;
using Microsoft.AspNetCore.Http.Features;

namespace Tanka.GraphQL.Server;

public record GraphQLRequestContext : QueryContext
{
    private FeatureReferences<FeatureInterfaces> _features;

    public GraphQLRequestContext(IFeatureCollection features) : base(features)
    {
        _features.Initalize(features);
    }

    public GraphQLRequestContext(): this(new FeatureCollection(5))
    {
    }

    public IServiceProvider RequestServices
    {
        get => ServiceProvidersFeature.RequestServices;
        set => ServiceProvidersFeature.RequestServices = value;
    } 

    private IRequestServicesFeature ServiceProvidersFeature =>
        _features.Fetch(ref _features.Cache.ServiceProviders, _ => new RequestServicesFeature())!;

    private struct FeatureInterfaces
    {
        public IRequestServicesFeature? ServiceProviders;
    }
}