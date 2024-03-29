﻿//HintName: InputMessageInputType.g.cs
/// <auto-generated/>

#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Tanka.GraphQL.Server;
using Tanka.GraphQL.Executable;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.Fields;

public static class InputMessageInputTypeExtensions
{
    public static SourceGeneratedTypesBuilder AddInputMessageInputType(this SourceGeneratedTypesBuilder builder)
    {
        builder.Builder.Configure(options => options.Builder.Add("""
                input InputMessage
                {
                    content: Person!
                }
                """));
        return builder;
    }
}

public partial class InputMessage : IParseableInputObject
{
    public void Parse(IReadOnlyDictionary<string, object?> argumentValue)
    {
        // Content is an input object type
        if (argumentValue.TryGetValue("content", out var contentValue))
        {
            if (contentValue is null)
            {
                Content = default;
            }
            else
            {
                if (contentValue is not IReadOnlyDictionary<string, object?> dictionaryValue)
                    throw new InvalidOperationException($"content is not IReadOnlyDictionary<string, object?>");
                Content = new Person();
                if (Content is not IParseableInputObject parseable)
                    throw new InvalidOperationException($"Content is not IParseableInputObject");
                parseable.Parse(dictionaryValue);
            }
        }
    }
}
#nullable restore
