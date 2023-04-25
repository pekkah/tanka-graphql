//HintName: TestsControllerExtensions.cs
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Tanka.GraphQL.Server;
using Tanka.GraphQL.Executable;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.Fields;

namespace Tests;
public static class TestsSourceGeneratedTypesExtensions
{
    public static SourceGeneratedTypesBuilder AddTestsControllers(this SourceGeneratedTypesBuilder builder)
    {
        builder.AddQueryController();
        builder.AddPersonController();
        return builder;
    }
}
