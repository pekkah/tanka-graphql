//HintName: QueryController.g.cs
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Tanka.GraphQL.Server;
using Tanka.GraphQL.Executable;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.Fields;



public static class QueryController
{


    public static ValueTask Person(ResolverContext context)
    {
        context.ResolvedValue = Query.Person(
                    context.GetArgument<int>("id")
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
                { "person(id: Int!): Person!", QueryController.Person }

            }));

        return builder;
    }
}
