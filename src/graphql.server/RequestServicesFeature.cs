using System;

namespace Tanka.GraphQL.Server;

public class RequestServicesFeature : IRequestServicesFeature
{
    public IServiceProvider RequestServices { get; set; }
}

public interface IRequestServicesFeature
{
    public IServiceProvider RequestServices { get; set; }
}