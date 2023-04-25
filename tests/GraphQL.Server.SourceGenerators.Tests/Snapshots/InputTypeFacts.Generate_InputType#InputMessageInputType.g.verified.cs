//HintName: InputMessageInputType.g.cs
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Tanka.GraphQL.Server;
using Tanka.GraphQL.Executable;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.Fields;



public static class InputMessageInputTypeExtensions
{
    public static SourceGeneratedTypesBuilder AddInputMessageInputType(
        this SourceGeneratedTypesBuilder builder)
    {
        builder.Builder.Configure(options => options.Builder.Add(
                """
                input InputMessage
                {
                    id: String!
                    content: String!
                }
                """
            );

        return builder;
    }
}
