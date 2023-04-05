using System.Runtime.CompilerServices;

namespace Tanka.GraphQL.Server.SourceGenerators.Tests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifySourceGenerators.Initialize();
    }
}