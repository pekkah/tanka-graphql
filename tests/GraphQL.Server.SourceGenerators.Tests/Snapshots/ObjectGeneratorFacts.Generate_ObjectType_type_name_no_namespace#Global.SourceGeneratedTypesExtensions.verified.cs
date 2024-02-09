//HintName: Global.SourceGeneratedTypesExtensions.cs
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.Server;
using Tanka.GraphQL.Executable;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.Fields;
public static class GlobalSourceGeneratedTypesExtensions
{
    public static SourceGeneratedTypesBuilder AddGlobalTypes(this SourceGeneratedTypesBuilder builder)
    {
        builder.AddQueryController();
        builder.AddPersonController();
        return builder;
    }
}
