﻿//HintName: PersonController.g.cs
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Tanka.GraphQL.Executable;
using Tanka.GraphQL.Fields;
using Tanka.GraphQL.Server;
using Tanka.GraphQL.ValueResolution;




public static class PersonController
{
    public static ValueTask Name(ResolverContext context)
    {
        var objectValue = (Person)context.ObjectValue;
        context.ResolvedValue = objectValue.Name;
        return default;
    }




}

public static class PersonControllerExtensions
{
    public static SourceGeneratedTypesBuilder AddPersonController(
        this SourceGeneratedTypesBuilder builder)
    {
        builder.Builder.Configure(options => options.Builder.Add(
            "Person",
            new FieldsWithResolvers()
            {
                { "name: String!", PersonController.Name }

            }));

        return builder;
    }
}