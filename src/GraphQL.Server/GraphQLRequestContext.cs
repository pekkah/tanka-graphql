using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Tanka.GraphQL.Server;

public record GraphQLRequestContext : QueryContext
{
    private FeatureReferences<FeatureInterfaces> _features;

    public GraphQLRequestContext(IFeatureCollection features) : base(features)
    {
        _features.Initalize(features);
    }

    public GraphQLRequestContext() : this(new FeatureCollection(2))
    {
    }

    public HttpContext HttpContext
    {
        get => HttpContextFeature.HttpContext;
        set => HttpContextFeature.HttpContext = value;
    }

    private IRequestServicesFeature ServiceProvidersFeature =>
        _features.Fetch(ref _features.Cache.ServiceProviders, _ => new RequestServicesFeature())!;

    private IHttpContextFeature HttpContextFeature => _features.Fetch(ref _features.Cache.HttpContext, _ => new HttpContextFeature())!;

    private struct FeatureInterfaces
    {
        public IRequestServicesFeature? ServiceProviders;
        public IHttpContextFeature? HttpContext;
    }
}