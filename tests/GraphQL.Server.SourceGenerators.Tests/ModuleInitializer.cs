using System.Runtime.CompilerServices;

using DiffEngine;

namespace Tanka.GraphQL.Server.SourceGenerators.Tests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        DiffTools.UseOrder(
            DiffTool.VisualStudio,
            DiffTool.VisualStudioCode
            );

        VerifySourceGenerators.Initialize();
    }
}