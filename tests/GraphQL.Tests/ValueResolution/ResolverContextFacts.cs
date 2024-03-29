﻿using System;
using System.Collections.Generic;
using System.Linq;
using Tanka.GraphQL.Fields;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Tests.ValueResolution;

public class ResolverContextFacts
{
    private readonly FieldDefinition _field;
    private readonly ObjectDefinition _objectType;
    private readonly object _objectValue;
    private readonly ISchema _schema;
    private readonly FieldSelection _selection;
    private IReadOnlyCollection<FieldSelection> _fields;

    public ResolverContextFacts()
    {
        _objectType = "type Test";
        _objectValue = null;
        _field = "test: ID";
        _selection = new(null, "test", null, null, null);
        _schema = new SchemaBuilder()
            .Add("type Query")
            .Build(new()).Result;
    }

    [Fact]
    public void Get_double_argument()
    {
        /* Given */
        var arguments = new Dictionary<string, object>
        {
            { "double", 100.1D }
        };

        var sut = new ResolverContext
        {
            ObjectDefinition = _objectType,
            ObjectValue = _objectValue,
            Field = _field,
            Fields = _fields,
            Arguments = arguments,
            Selection = _selection,
            Path = new NodePath(),
            QueryContext = null!
        };

        /* When */
        var value = sut.GetArgument<double>("double");

        /* Then */
        Assert.Equal(100.1D, value);
    }

    [Fact]
    public void Get_float_argument()
    {
        /* Given */
        var arguments = new Dictionary<string, object>
        {
            { "float", 100.1F }
        };

        var sut = new ResolverContext
        {
            ObjectDefinition = _objectType,
            ObjectValue = _objectValue,
            Field = _field,
            Fields = _fields,
            Arguments = arguments,
            Selection = _selection,
            Path = new NodePath(),
            QueryContext = null!
        };

        /* When */
        var value = sut.GetArgument<float>("float");

        /* Then */
        Assert.Equal(100.1F, value);
    }

    [Fact]
    public void Get_int_argument()
    {
        /* Given */
        var arguments = new Dictionary<string, object>
        {
            { "int", 101 }
        };

        var sut = new ResolverContext
        {
            ObjectDefinition = _objectType,
            ObjectValue = _objectValue,
            Field = _field,
            Fields = _fields,
            Arguments = arguments,
            Selection = _selection,
            Path = new NodePath(),
            QueryContext = null!
        };

        /* When */
        var value = sut.GetArgument<int>("int");

        /* Then */
        Assert.Equal(101, value);
    }

    [Fact]
    public void Get_long_argument()
    {
        /* Given */
        var arguments = new Dictionary<string, object>
        {
            { "long", 100L }
        };

        var sut = new ResolverContext
        {
            ObjectDefinition = _objectType,
            ObjectValue = _objectValue,
            Field = _field,
            Fields = _fields,
            Arguments = arguments,
            Selection = _selection,
            Path = new NodePath(),
            QueryContext = null!
        };

        /* When */
        var value = sut.GetArgument<long>("long");

        /* Then */
        Assert.Equal(100L, value);
    }

    [Fact]
    public void Get_object_argument()
    {
        /* Given */
        var arguments = new Dictionary<string, object>
        {
            {
                "input", new Dictionary<string, object>
                {
                    { "name", "inputArg" }
                }
            }
        };

        var sut = new ResolverContext
        {
            ObjectDefinition = _objectType,
            ObjectValue = _objectValue,
            Field = _field,
            Fields = _fields,
            Arguments = arguments,
            Selection = _selection,
            Path = new NodePath(),
            QueryContext = null!
        };

        /* When */
        var value = sut.BindInputObject<InputArg>("input");

        /* Then */
        Assert.Equal("inputArg", value.Name);
    }

    [Fact]
    public void Get_InputObject_List_argument()
    {
        /* Given */
        var arguments = new Dictionary<string, object>
        {
            {
                "inputs", new[]
                {
                    new Dictionary<string, object>
                    {
                        { "name", "1" }
                    },
                    new Dictionary<string, object>
                    {
                        { "name", "2" }
                    }
                }
            }
        };

        var sut = new ResolverContext
        {
            ObjectDefinition = _objectType,
            ObjectValue = _objectValue,
            Field = _field,
            Fields = _fields,
            Arguments = arguments,
            Selection = _selection,
            Path = new NodePath(),
            QueryContext = null!
        };

        /* When */
        var value = sut.BindInputObjectList<InputArg>("inputs");

        /* Then */
        Assert.Single(value, v => v.Name == "1");
        Assert.Single(value, v => v.Name == "2");
    }

    [Fact]
    public void Get_InputObject_List_argument_with_null()
    {
        /* Given */
        var arguments = new Dictionary<string, object>
        {
            {
                "inputs", new[]
                {
                    new Dictionary<string, object>
                    {
                        { "name", "1" }
                    },
                    null,
                    new Dictionary<string, object>
                    {
                        { "name", "2" }
                    }
                }
            }
        };

        var sut = new ResolverContext
        {
            ObjectDefinition = _objectType,
            ObjectValue = _objectValue,
            Field = _field,
            Fields = _fields,
            Arguments = arguments,
            Selection = _selection,
            Path = new NodePath(),
            QueryContext = null!
        };

        /* When */
        var value = sut.BindInputObjectList<InputArg?>("inputs")
            .ToList();

        /* Then */
        Assert.Single(value, v => v?.Name == "1");
        Assert.Single(value, v => v?.Name == "2");
        Assert.Single(value, v => v is null);
    }

    [Fact]
    public void Get_string_argument()
    {
        /* Given */
        var arguments = new Dictionary<string, object>
        {
            { "string", "101" }
        };

        var sut = new ResolverContext
        {
            ObjectDefinition = _objectType,
            ObjectValue = _objectValue,
            Field = _field,
            Fields = _fields,
            Arguments = arguments,
            Selection = _selection,
            Path = new NodePath(),
            QueryContext = null!
        };

        /* When */
        var value = sut.GetArgument<string>("string");

        /* Then */
        Assert.Equal("101", value);
    }
}

public class InputArg
{
    public string Name { get; set; }
}