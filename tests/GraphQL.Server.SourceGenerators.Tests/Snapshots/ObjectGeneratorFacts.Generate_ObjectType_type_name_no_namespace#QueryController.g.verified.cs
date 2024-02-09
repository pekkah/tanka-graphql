﻿//HintName: QueryController.g.cs
/// <auto-generated/>
#nullable enable

using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Tanka.GraphQL.Executable;
using Tanka.GraphQL.Fields;
using Tanka.GraphQL.Server;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;


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
            }            ));

        return builder;
    }
}

public static partial class Query
{
    public static string __Typename => "Query";
}

#nullable restore