using System;

namespace Tanka.GraphQL.Tests;

public class EmptyServiceProvider : IServiceProvider
{
    public object? GetService(Type serviceType)
    {
        return null;
    }

    public static IServiceProvider Instance { get; } = new EmptyServiceProvider();
}