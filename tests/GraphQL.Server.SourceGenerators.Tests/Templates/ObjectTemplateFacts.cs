using Tanka.GraphQL.Server.SourceGenerators.Internal;
using Tanka.GraphQL.Server.SourceGenerators.Templates;

namespace Tanka.GraphQL.Server.SourceGenerators.Tests.Templates;

[UsesVerify]
public class ObjectTemplateFacts
{
    [Fact]
    public async Task With_property()
    {
        /* Given */
        var template = new ObjectTemplate
        {
            Namespace = "Tests",
            Name = "Dog",
            TypeName = "Dog",
            Usings =
            [
                "using Animals;"
            ],
            Methods = [],
            NamedTypeExtension = NamedTypeExtension.Render("class", "Dog", "Dog"),
            Properties =
            [
                new ObjectPropertyDefinition
                {
                    Name = "Name",
                    ReturnType = "string",
                    ClosestMatchingGraphQLTypeName = "String!"
                }
            ]
        };

        /* When */
        var actual = template.Render();

        /* Then */
        await VerifyTemplate(actual);
    }

    [Fact]
    public async Task Without_namespace()
    {
        /* Given */
        var template = new ObjectTemplate
        {
            Namespace = "",
            Name = "Dog",
            TypeName = "Dog",
            Usings =
            [
                "using Animals;"
            ],
            Methods = [],
            NamedTypeExtension = NamedTypeExtension.Render("class", "Dog", "Dog"),
            Properties =
            [
                new ObjectPropertyDefinition
                {
                    Name = "Name",
                    ReturnType = "string",
                    ClosestMatchingGraphQLTypeName = "String!"
                }
            ]
        };

        /* When */
        var actual = template.Render();

        /* Then */
        await VerifyTemplate(actual);
    }

    [Fact]
    public async Task With_multiple_properties()
    {
        /* Given */
        var template = new ObjectTemplate
        {
            Namespace = "Tests",
            Name = "Dog",
            TypeName = "Dog",
            Usings =
            [
                "using Animals;"
            ],
            Methods = [],
            NamedTypeExtension = NamedTypeExtension.Render("class", "Dog", "Dog"),
            Properties =
            [
                new ObjectPropertyDefinition
                {
                    Name = "Name",
                    ReturnType = "string",
                    ClosestMatchingGraphQLTypeName = "String!"
                },
                new ObjectPropertyDefinition
                {
                    Name = "Age",
                    ReturnType = "int",
                    ClosestMatchingGraphQLTypeName = "Int!"
                }
            ]
        };

        /* When */
        var actual = template.Render();

        /* Then */
        await VerifyTemplate(actual);
    }

    [Fact]
    public async Task With_method()
    {
        /* Given */
        var template = new ObjectTemplate
        {
            Namespace = "Tests",
            Name = "Dog",
            TypeName = "Dog",
            Usings =
            [
                "using Animals;"
            ],
            NamedTypeExtension = NamedTypeExtension.Render("class", "Dog", "Dog"),
            Methods = 
            [
                new ObjectMethodDefinition()
                {
                    IsStatic = false,
                    Name = "Method",
                    ReturnType = "string",
                    Type = MethodType.T,
                    ClosestMatchingGraphQLTypeName = "String!"
                }
            ],
            Properties = [],
        };

        /* When */
        var actual = template.Render();

        /* Then */
        await VerifyTemplate(actual);
    }

    [Fact]
    public async Task With_method_with_ResolverContext_parameter()
    {
        /* Given */
        var template = new ObjectTemplate
        {
            Namespace = "Tests",
            Name = "Dog",
            TypeName = "Dog",
            Usings =
            [
                "using Animals;"
            ],
            NamedTypeExtension = NamedTypeExtension.Render("class", "Dog", "Dog"),
            Methods =
            [
                new ObjectMethodDefinition()
                {
                    IsStatic = false,
                    Name = "Method",
                    ReturnType = "string",
                    Type = MethodType.T,
                    ClosestMatchingGraphQLTypeName = "String!",
                    Parameters = new []
                    {
                        new ParameterDefinition()
                        {
                            Name = "context",
                            Type = "ResolverContext",
                        }
                    }.ToEquatableArray()
                }
            ],
            Properties = [],
        };

        /* When */
        var actual = template.Render();

        /* Then */
        await VerifyTemplate(actual);
    }

    [Fact]
    public async Task With_method_with_CancellationToken_parameter()
    {
        /* Given */
        var template = new ObjectTemplate
        {
            Namespace = "Tests",
            Name = "Dog",
            TypeName = "Dog",
            Usings =
            [
                "using Animals;"
            ],
            NamedTypeExtension = NamedTypeExtension.Render("class", "Dog", "Dog"),
            Methods =
            [
                new ObjectMethodDefinition()
                {
                    IsStatic = false,
                    Name = "Method",
                    ReturnType = "string",
                    Type = MethodType.T,
                    ClosestMatchingGraphQLTypeName = "String!",
                    Parameters = new []
                    {
                        new ParameterDefinition()
                        {
                            Name = "cancellationToken",
                            Type = "CancellationToken",
                        }
                    }.ToEquatableArray()
                }
            ],
            Properties = [],
        };

        /* When */
        var actual = template.Render();

        /* Then */
        await VerifyTemplate(actual);
    }

    [Fact]
    public async Task With_method_with_IServiceProvider_parameter()
    {
        /* Given */
        var template = new ObjectTemplate
        {
            Namespace = "Tests",
            Name = "Dog",
            TypeName = "Dog",
            Usings =
            [
                "using Animals;"
            ],
            NamedTypeExtension = NamedTypeExtension.Render("class", "Dog", "Dog"),
            Methods =
            [
                new ObjectMethodDefinition()
                {
                    IsStatic = false,
                    Name = "Method",
                    ReturnType = "string",
                    Type = MethodType.T,
                    ClosestMatchingGraphQLTypeName = "String!",
                    Parameters = new []
                    {
                        new ParameterDefinition()
                        {
                            Name = "provider",
                            Type = "IServiceProvider",
                        }
                    }.ToEquatableArray()
                }
            ],
            Properties = [],
        };

        /* When */
        var actual = template.Render();

        /* Then */
        await VerifyTemplate(actual);
    }

    [Fact]
    public async Task With_method_with_FromArguments_parameter()
    {
        /* Given */
        var template = new ObjectTemplate
        {
            Namespace = "Tests",
            Name = "Dog",
            TypeName = "Dog",
            Usings =
            [
                "using Animals;"
            ],
            NamedTypeExtension = NamedTypeExtension.Render("class", "Dog", "Dog"),
            Methods =
            [
                new ObjectMethodDefinition()
                {
                    IsStatic = false,
                    Name = "Method",
                    ReturnType = "string",
                    Type = MethodType.T,
                    ClosestMatchingGraphQLTypeName = "String!",
                    Parameters = new []
                    {
                        new ParameterDefinition()
                        {
                            Name = "object",
                            Type = "Person",
                            ClosestMatchingGraphQLTypeName = "Person!",
                            FromArguments = true,
                            IsPrimitive = false
                        },
                        new ParameterDefinition()
                        {
                            Name = "primitive",
                            Type = "int",
                            ClosestMatchingGraphQLTypeName = "Int!",
                            FromArguments = true,
                            IsPrimitive = true
                        }
                    }.ToEquatableArray()
                }
            ],
            Properties = [],
        };

        /* When */
        var actual = template.Render();

        /* Then */
        await VerifyTemplate(actual);
    }

    [Fact]
    public async Task With_method_with_FromServices_parameter()
    {
        /* Given */
        var template = new ObjectTemplate
        {
            Namespace = "Tests",
            Name = "Dog",
            TypeName = "Dog",
            Usings =
            [
                "using Animals;"
            ],
            NamedTypeExtension = NamedTypeExtension.Render("class", "Dog", "Dog"),
            Methods =
            [
                new ObjectMethodDefinition()
                {
                    IsStatic = false,
                    Name = "Method",
                    ReturnType = "string",
                    Type = MethodType.T,
                    ClosestMatchingGraphQLTypeName = "String!",
                    Parameters = new []
                    {
                        new ParameterDefinition()
                        {
                            Name = "service",
                            Type = "IService?",
                            IsNullable = true,
                            FromServices = true
                        },
                        new ParameterDefinition()
                        {
                            Name = "requiredService",
                            Type = "IService",
                            IsNullable = false,
                            FromServices = true
                        }
                    }.ToEquatableArray()
                }
            ],
            Properties = [],
        };

        /* When */
        var actual = template.Render();

        /* Then */
        await VerifyTemplate(actual);
    }

    [Fact]
    public async Task With_method_with_IsPrimitive_parameter()
    {
        /* Given */
        var template = new ObjectTemplate
        {
            Namespace = "Tests",
            Name = "Dog",
            TypeName = "Dog",
            Usings =
            [
                "using Animals;"
            ],
            NamedTypeExtension = NamedTypeExtension.Render("class", "Dog", "Dog"),
            Methods =
            [
                new ObjectMethodDefinition()
                {
                    IsStatic = false,
                    Name = "Method",
                    ReturnType = "string",
                    Type = MethodType.T,
                    ClosestMatchingGraphQLTypeName = "String!",
                    Parameters = new []
                    {
                        new ParameterDefinition()
                        {
                            Name = "primitive",
                            Type = "int",
                            ClosestMatchingGraphQLTypeName = "Int!",
                            IsPrimitive = true
                        }
                    }.ToEquatableArray()
                }
            ],
            Properties = [],
        };

        /* When */
        var actual = template.Render();

        /* Then */
        await VerifyTemplate(actual);
    }

    [Fact]
    public async Task With_method_with_context_and_CancellationToken_parameters()
    {
        /* Given */
        var template = new ObjectTemplate
        {
            Namespace = "Tests",
            Name = "Dog",
            TypeName = "Dog",
            Usings =
            [
                "using Animals;"
            ],
            NamedTypeExtension = NamedTypeExtension.Render("class", "Dog", "Dog"),
            Methods =
            [
                new ObjectMethodDefinition()
                {
                    IsStatic = false,
                    Name = "Method",
                    ReturnType = "string",
                    Type = MethodType.T,
                    ClosestMatchingGraphQLTypeName = "String!",
                    Parameters = new []
                    {
                        new ParameterDefinition()
                        {
                            Name = "context",
                            Type = "ResolverContext",
                        },
                        new ParameterDefinition()
                        {
                            Name = "cancellationToken",
                            Type = "CancellationToken",
                        }
                    }.ToEquatableArray()
                }
            ],
            Properties = [],
        };

        /* When */
        var actual = template.Render();

        /* Then */
        await VerifyTemplate(actual);
    }

    [Fact]
    public async Task With_static_method()
    {
        /* Given */
        var template = new ObjectTemplate
        {
            Namespace = "Tests",
            Name = "Dog",
            TypeName = "Dog",
            Usings =
            [
                "using Animals;"
            ],
            Methods =
            [
                new ObjectMethodDefinition()
                {
                    IsStatic = true,
                    Name = "Method",
                    ReturnType = "string",
                    Type = MethodType.T,
                    ClosestMatchingGraphQLTypeName = "String!",
                }
            ],
            NamedTypeExtension = NamedTypeExtension.Render("class", "Dog", "Dog"),
            Properties = []
        };

        /* When */
        var actual = template.Render();

        /* Then */
        await VerifyTemplate(actual);
    }

    [Fact]
    public async Task With_multiple_methods()
    {
        /* Given */
        var template = new ObjectTemplate
        {
            Namespace = "Tests",
            Name = "Dog",
            TypeName = "Dog",
            Usings =
            [
                "using Animals;"
            ],
            Methods =
            [
                new ObjectMethodDefinition()
                {
                    IsStatic = true,
                    Name = "Method",
                    ReturnType = "string",
                    Type = MethodType.T,
                    ClosestMatchingGraphQLTypeName = "String!",
                },
                new ObjectMethodDefinition()
                {
                    IsStatic = false,
                    Name = "Method2",
                    ReturnType = "double",
                    Type = MethodType.T,
                    ClosestMatchingGraphQLTypeName = "Float!",
                }
            ],
            NamedTypeExtension = NamedTypeExtension.Render("class", "Dog", "Dog"),
            Properties = []
        };

        /* When */
        var actual = template.Render();

        /* Then */
        await VerifyTemplate(actual);
    }

    [Fact]
    public async Task With_instance_property_and_method()
    {
        /* Given */
        var template = new ObjectTemplate
        {
            Namespace = "Tests",
            Name = "Dog",
            TypeName = "Dog",
            Usings =
            [
                "using Animals;"
            ],
            Methods =
            [
                new ObjectMethodDefinition()
                {
                    IsStatic = false,
                    Name = "Method",
                    ReturnType = "string",
                    Type = MethodType.T,
                    ClosestMatchingGraphQLTypeName = "String!",
                }
            ],
            Properties =
            [
                new ObjectPropertyDefinition()
                {
                    ClosestMatchingGraphQLTypeName = "Int!",
                    Name = "Property",
                    ReturnType = "int",
                    IsStatic = false
                }
            ],
            NamedTypeExtension = NamedTypeExtension.Render("class", "Dog", "Dog"),
        };

        /* When */
        var actual = template.Render();

        /* Then */
        await VerifyTemplate(actual);
    }

    [Fact]
    public async Task With_subscriber_method()
    {
        /* Given */
        var template = new ObjectTemplate
        {
            Namespace = "Tests",
            Name = "Dog",
            TypeName = "Dog",
            Usings =
            [
                "using Animals;"
            ],
            NamedTypeExtension = NamedTypeExtension.Render("class", "Dog", "Dog"),
            Methods =
            [
                new ObjectMethodDefinition()
                {
                    IsStatic = false,
                    Name = "Method",
                    ReturnType = "string",
                    Type = MethodType.AsyncEnumerableOfT,
                    ClosestMatchingGraphQLTypeName = "String!"
                }
            ],
            Properties = [],
        };

        /* When */
        var actual = template.Render();

        /* Then */
        await VerifyTemplate(actual);
    }
}
