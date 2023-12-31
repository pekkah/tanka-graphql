using Microsoft.Extensions.DependencyInjection;

namespace Tanka.GraphQL;

public class RequestServicesFeature : IRequestServicesFeature
{
    private static IServiceProvider EmptyServices { get; } = new ServiceCollection().BuildServiceProvider();
    
    public IServiceProvider RequestServices { get; set; } = EmptyServices;
}

public interface IRequestServicesFeature
{
    public IServiceProvider RequestServices { get; set; }
}