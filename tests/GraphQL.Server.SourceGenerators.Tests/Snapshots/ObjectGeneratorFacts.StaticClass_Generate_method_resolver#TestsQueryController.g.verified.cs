//HintName: TestsQueryController.g.cs
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Tanka.GraphQL.Server;
using Tanka.GraphQL.Executable;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.Fields;

namespace Tests;

public static class QueryController
{


    public static ValueTask Id(ResolverContext context)
    {
        context.ResolvedValue = Query.Id(
                    context.GetArgument<int?>("p1")
                    );

        return default;
    }


}

public static class QueryControllerExtensions
{
    public static SourceGeneratedTypesBuilder AddQueryController(
        this SourceGeneratedTypesBuilder builder)
    {
        builder.Builder.Configure(options => options.Builder.Add(
            "Query",
            new FieldsWithResolvers()
            {
                { "id(p1: Int): Int!", QueryController.Id }

            }));

        return builder;
    }
}
