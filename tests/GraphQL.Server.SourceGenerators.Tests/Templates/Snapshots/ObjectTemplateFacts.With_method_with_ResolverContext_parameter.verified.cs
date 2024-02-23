﻿/// <auto-generated/>
#nullable enable

using Animals;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Tanka.GraphQL.Executable;
using Tanka.GraphQL.Fields;
using Tanka.GraphQL.Server;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;

namespace Tests;

public static partial class DogController
{
    public static ValueTask Method(ResolverContext context)
    {
        BeforeMethod(context);
        
        context.ResolvedValue = ((Dog)context.ObjectValue).Method(
            context
            );
        
        AfterMethod(context);
        return default;
    }
    static partial void BeforeMethod(ResolverContext context);
    static partial void AfterMethod(ResolverContext context);
}

public static class DogControllerExtensions
{
    public static SourceGeneratedTypesBuilder AddDogController(
        this SourceGeneratedTypesBuilder builder)
    {
        builder.Builder.Configure(options => options.Builder.Add(
            "Dog",
            new FieldsWithResolvers()
            {
                { "method: String!", DogController.Method }
            }            ));
            
            
        return builder;
    }
}

public partial class Dog: INamedType
{
    public string __Typename => "Dog";
}

#nullable restore