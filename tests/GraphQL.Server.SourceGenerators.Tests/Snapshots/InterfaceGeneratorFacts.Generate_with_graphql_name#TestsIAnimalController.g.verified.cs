﻿//HintName: TestsIAnimalController.g.cs
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

namespace Tests;

public static class IAnimalControllerExtensions
{
    public static SourceGeneratedTypesBuilder AddIAnimalController(
        this SourceGeneratedTypesBuilder builder)
    {
        builder.Builder.Configure(options => options.Builder.Add(
            """
            interface Animal
            {
                name: String!
            }
            """));

        return builder;
    }
}

public partial interface IAnimal: INamedType
{
    public string __Typename => "Animal";
}

#nullable restore