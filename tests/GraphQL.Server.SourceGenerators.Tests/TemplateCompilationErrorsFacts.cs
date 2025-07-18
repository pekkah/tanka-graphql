using Tanka.GraphQL.Server.SourceGenerators.Internal;
using Tanka.GraphQL.Server.SourceGenerators.Templates;

namespace Tanka.GraphQL.Server.SourceGenerators.Tests;

public class TemplateCompilationErrorsFacts
{
    [Fact]
    public async Task ObjectTemplate_WithInvalidCSharpSyntax()
    {
        /* Given */
        var template = new ObjectTemplate
        {
            Namespace = "Tests",
            Name = "Dog",
            TypeName = "Dog",
            Usings = ["using System;"],
            Methods = [],
            NamedTypeExtension = NamedTypeExtension.Render("class", "Dog", "Dog"),
            Properties = []
        };

        /* When */
        var actual = template.Render();

        /* Then */
        await VerifyTemplate(actual);
    }

    [Fact]
    public async Task ObjectTemplate_WithNullNamespace()
    {
        /* Given */
        var template = new ObjectTemplate
        {
            Namespace = null,
            Name = "Dog",
            TypeName = "Dog",
            Usings = ["using System;"],
            Methods = [],
            NamedTypeExtension = NamedTypeExtension.Render("class", "Dog", "Dog"),
            Properties = []
        };

        /* When */
        var actual = template.Render();

        /* Then */
        await VerifyTemplate(actual);
    }

    [Fact]
    public async Task ObjectTemplate_WithEmptyName()
    {
        /* Given */
        var template = new ObjectTemplate
        {
            Namespace = "Tests",
            Name = "",
            TypeName = "",
            Usings = ["using System;"],
            Methods = [],
            NamedTypeExtension = NamedTypeExtension.Render("class", "", ""),
            Properties = []
        };

        /* When */
        var actual = template.Render();

        /* Then */
        await VerifyTemplate(actual);
    }

    [Fact]
    public async Task ObjectTemplate_WithInvalidPropertyTypes()
    {
        /* Given */
        var template = new ObjectTemplate
        {
            Namespace = "Tests",
            Name = "Dog",
            TypeName = "Dog",
            Usings = ["using System;"],
            Methods = [],
            NamedTypeExtension = NamedTypeExtension.Render("class", "Dog", "Dog"),
            Properties = [
                new ObjectPropertyDefinition
                {
                    Name = "InvalidProperty",
                    ReturnType = "NonExistentType",
                    ClosestMatchingGraphQLTypeName = "NonExistentType!"
                }
            ]
        };

        /* When */
        var actual = template.Render();

        /* Then */
        await VerifyTemplate(actual);
    }

    [Fact]
    public async Task ObjectTemplate_WithInvalidMethodReturnTypes()
    {
        /* Given */
        var template = new ObjectTemplate
        {
            Namespace = "Tests",
            Name = "Dog",
            TypeName = "Dog",
            Usings = ["using System;"],
            NamedTypeExtension = NamedTypeExtension.Render("class", "Dog", "Dog"),
            Methods = [
                new ObjectMethodDefinition
                {
                    IsStatic = false,
                    Name = "InvalidMethod",
                    ReturnType = "NonExistentType",
                    Type = MethodType.T,
                    ClosestMatchingGraphQLTypeName = "NonExistentType!"
                }
            ],
            Properties = []
        };

        /* When */
        var actual = template.Render();

        /* Then */
        await VerifyTemplate(actual);
    }

    [Fact]
    public async Task ObjectTemplate_WithInvalidParameterTypes()
    {
        /* Given */
        var template = new ObjectTemplate
        {
            Namespace = "Tests",
            Name = "Dog",
            TypeName = "Dog",
            Usings = ["using System;"],
            NamedTypeExtension = NamedTypeExtension.Render("class", "Dog", "Dog"),
            Methods = [
                new ObjectMethodDefinition
                {
                    IsStatic = false,
                    Name = "MethodWithInvalidParameter",
                    ReturnType = "string",
                    Type = MethodType.T,
                    ClosestMatchingGraphQLTypeName = "String!",
                    Parameters = [
                        new ParameterDefinition
                        {
                            Name = "invalidParam",
                            Type = "NonExistentType",
                            ClosestMatchingGraphQLTypeName = "NonExistentType!"
                        }
                    ].ToEquatableArray()
                }
            ],
            Properties = []
        };

        /* When */
        var actual = template.Render();

        /* Then */
        await VerifyTemplate(actual);
    }

    [Fact]
    public async Task ObjectTemplate_WithDuplicateNames()
    {
        /* Given */
        var template = new ObjectTemplate
        {
            Namespace = "Tests",
            Name = "Dog",
            TypeName = "Dog",
            Usings = ["using System;"],
            NamedTypeExtension = NamedTypeExtension.Render("class", "Dog", "Dog"),
            Methods = [
                new ObjectMethodDefinition
                {
                    IsStatic = false,
                    Name = "DuplicateName",
                    ReturnType = "string",
                    Type = MethodType.T,
                    ClosestMatchingGraphQLTypeName = "String!"
                },
                new ObjectMethodDefinition
                {
                    IsStatic = false,
                    Name = "DuplicateName",
                    ReturnType = "int",
                    Type = MethodType.T,
                    ClosestMatchingGraphQLTypeName = "Int!"
                }
            ],
            Properties = [
                new ObjectPropertyDefinition
                {
                    Name = "DuplicateName",
                    ReturnType = "bool",
                    ClosestMatchingGraphQLTypeName = "Boolean!"
                }
            ]
        };

        /* When */
        var actual = template.Render();

        /* Then */
        await VerifyTemplate(actual);
    }

    [Fact]
    public async Task ObjectTemplate_WithInvalidUsings()
    {
        /* Given */
        var template = new ObjectTemplate
        {
            Namespace = "Tests",
            Name = "Dog",
            TypeName = "Dog",
            Usings = [
                "using System;",
                "using NonExistent.Namespace;",
                "using Another.Invalid.Namespace;"
            ],
            Methods = [],
            NamedTypeExtension = NamedTypeExtension.Render("class", "Dog", "Dog"),
            Properties = []
        };

        /* When */
        var actual = template.Render();

        /* Then */
        await VerifyTemplate(actual);
    }

    [Fact]
    public async Task ObjectTemplate_WithInvalidImplements()
    {
        /* Given */
        var template = new ObjectTemplate
        {
            Namespace = "Tests",
            Name = "Dog",
            TypeName = "Dog",
            Usings = ["using System;"],
            NamedTypeExtension = NamedTypeExtension.Render("class", "Dog", "Dog"),
            Methods = [],
            Properties = [],
            Implements = [
                new BaseDefinition(true, "NonExistentInterface", "NonExistent.Namespace", "NonExistentInterface", [], [])
            ]
        };

        /* When */
        var actual = template.Render();

        /* Then */
        await VerifyTemplate(actual);
    }

    [Fact]
    public async Task ObjectTemplate_WithCircularDependencies()
    {
        /* Given */
        var template = new ObjectTemplate
        {
            Namespace = "Tests",
            Name = "Dog",
            TypeName = "Dog",
            Usings = ["using System;"],
            NamedTypeExtension = NamedTypeExtension.Render("class", "Dog", "Dog"),
            Methods = [],
            Properties = [
                new ObjectPropertyDefinition
                {
                    Name = "Parent",
                    ReturnType = "Dog",
                    ClosestMatchingGraphQLTypeName = "Dog!"
                },
                new ObjectPropertyDefinition
                {
                    Name = "Children",
                    ReturnType = "List<Dog>",
                    ClosestMatchingGraphQLTypeName = "[Dog!]!"
                }
            ]
        };

        /* When */
        var actual = template.Render();

        /* Then */
        await VerifyTemplate(actual);
    }

    [Fact]
    public async Task ObjectTemplate_WithInvalidGraphQLNames()
    {
        /* Given */
        var template = new ObjectTemplate
        {
            Namespace = "Tests",
            Name = "Dog",
            TypeName = "Dog",
            Usings = ["using System;"],
            NamedTypeExtension = NamedTypeExtension.Render("class", "Dog", "Dog"),
            Methods = [
                new ObjectMethodDefinition
                {
                    IsStatic = false,
                    Name = "ValidMethod",
                    ReturnType = "string",
                    Type = MethodType.T,
                    ClosestMatchingGraphQLTypeName = "123InvalidGraphQLName!"
                }
            ],
            Properties = [
                new ObjectPropertyDefinition
                {
                    Name = "ValidProperty",
                    ReturnType = "string",
                    ClosestMatchingGraphQLTypeName = "456AnotherInvalidName!"
                }
            ]
        };

        /* When */
        var actual = template.Render();

        /* Then */
        await VerifyTemplate(actual);
    }

    [Fact]
    public async Task ObjectTemplate_WithAsyncMethodsAndInvalidTypes()
    {
        /* Given */
        var template = new ObjectTemplate
        {
            Namespace = "Tests",
            Name = "Dog",
            TypeName = "Dog",
            Usings = ["using System;", "using System.Threading.Tasks;"],
            NamedTypeExtension = NamedTypeExtension.Render("class", "Dog", "Dog"),
            Methods = [
                new ObjectMethodDefinition
                {
                    IsStatic = false,
                    Name = "AsyncMethod",
                    ReturnType = "Task<NonExistentType>",
                    Type = MethodType.TaskOfT,
                    ClosestMatchingGraphQLTypeName = "NonExistentType!"
                },
                new ObjectMethodDefinition
                {
                    IsStatic = false,
                    Name = "ValueTaskMethod",
                    ReturnType = "ValueTask<AnotherNonExistentType>",
                    Type = MethodType.ValueTaskOfT,
                    ClosestMatchingGraphQLTypeName = "AnotherNonExistentType!"
                }
            ],
            Properties = []
        };

        /* When */
        var actual = template.Render();

        /* Then */
        await VerifyTemplate(actual);
    }

    [Fact]
    public async Task ObjectTemplate_WithSubscriberMethodsAndInvalidTypes()
    {
        /* Given */
        var template = new ObjectTemplate
        {
            Namespace = "Tests",
            Name = "Dog",
            TypeName = "Dog",
            Usings = ["using System;", "using System.Collections.Generic;"],
            NamedTypeExtension = NamedTypeExtension.Render("class", "Dog", "Dog"),
            Methods = [
                new ObjectMethodDefinition
                {
                    IsStatic = false,
                    Name = "SubscriberMethod",
                    ReturnType = "IAsyncEnumerable<InvalidType>",
                    Type = MethodType.AsyncEnumerableOfT,
                    ClosestMatchingGraphQLTypeName = "InvalidType!"
                }
            ],
            Properties = []
        };

        /* When */
        var actual = template.Render();

        /* Then */
        await VerifyTemplate(actual);
    }

    [Fact]
    public async Task ObjectTemplate_WithComplexInvalidScenario()
    {
        /* Given */
        var template = new ObjectTemplate
        {
            Namespace = "Very.Long.And.Potentially.Invalid.Namespace",
            Name = "ComplexInvalidType",
            TypeName = "ComplexInvalidType",
            Usings = [
                "using System;",
                "using NonExistent.Namespace;",
                "using Another.Invalid.Namespace;",
                "using System.Collections.Generic;",
                "using System.Threading.Tasks;"
            ],
            NamedTypeExtension = NamedTypeExtension.Render("class", "ComplexInvalidType", "ComplexInvalidType"),
            Methods = [
                new ObjectMethodDefinition
                {
                    IsStatic = false,
                    Name = "ComplexMethod",
                    ReturnType = "Task<List<Dictionary<string, InvalidType>>>",
                    Type = MethodType.TaskOfT,
                    ClosestMatchingGraphQLTypeName = "[InvalidType!]!",
                    Parameters = [
                        new ParameterDefinition
                        {
                            Name = "invalidParam1",
                            Type = "NonExistentType",
                            ClosestMatchingGraphQLTypeName = "NonExistentType!",
                            FromArguments = true
                        },
                        new ParameterDefinition
                        {
                            Name = "invalidParam2",
                            Type = "AnotherInvalidType",
                            ClosestMatchingGraphQLTypeName = "AnotherInvalidType",
                            FromServices = true
                        }
                    ].ToEquatableArray()
                }
            ],
            Properties = [
                new ObjectPropertyDefinition
                {
                    Name = "InvalidProperty",
                    ReturnType = "List<Dictionary<string, YetAnotherInvalidType>>",
                    ClosestMatchingGraphQLTypeName = "[YetAnotherInvalidType!]!"
                }
            ],
            Implements = [
                new BaseDefinition(true, "IInvalidInterface", "Invalid.Namespace", "IInvalidInterface", [], [])
            ]
        };

        /* When */
        var actual = template.Render();

        /* Then */
        await VerifyTemplate(actual);
    }

    [Fact]
    public async Task InterfaceTemplate_WithInvalidTypes()
    {
        /* Given */
        var template = new InterfaceTemplate
        {
            Namespace = "Tests",
            Name = "IInvalidInterface",
            TypeName = "IInvalidInterface",
            Usings = ["using System;"],
            NamedTypeExtension = NamedTypeExtension.Render("interface", "IInvalidInterface", "IInvalidInterface"),
            Methods = [
                new ObjectMethodDefinition
                {
                    IsStatic = false,
                    Name = "InvalidMethod",
                    ReturnType = "NonExistentType",
                    Type = MethodType.T,
                    ClosestMatchingGraphQLTypeName = "NonExistentType!"
                }
            ],
            Properties = [
                new ObjectPropertyDefinition
                {
                    Name = "InvalidProperty",
                    ReturnType = "AnotherNonExistentType",
                    ClosestMatchingGraphQLTypeName = "AnotherNonExistentType!"
                }
            ]
        };

        /* When */
        var actual = template.Render();

        /* Then */
        await VerifyTemplate(actual);
    }

    [Fact]
    public async Task Template_WithExtremelyLongNames()
    {
        /* Given */
        var extremelyLongName = new string('A', 1000);
        var template = new ObjectTemplate
        {
            Namespace = "Tests",
            Name = extremelyLongName,
            TypeName = extremelyLongName,
            Usings = ["using System;"],
            NamedTypeExtension = NamedTypeExtension.Render("class", extremelyLongName, extremelyLongName),
            Methods = [],
            Properties = [
                new ObjectPropertyDefinition
                {
                    Name = extremelyLongName,
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
    public async Task Template_WithSpecialCharacters()
    {
        /* Given */
        var template = new ObjectTemplate
        {
            Namespace = "Tests",
            Name = "Type_With_Special_Characters",
            TypeName = "Type_With_Special_Characters",
            Usings = ["using System;"],
            NamedTypeExtension = NamedTypeExtension.Render("class", "Type_With_Special_Characters", "Type_With_Special_Characters"),
            Methods = [],
            Properties = [
                new ObjectPropertyDefinition
                {
                    Name = "Property_With_Underscores",
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

    private static async Task VerifyTemplate(string actual)
    {
        await Verifier.Verify(actual).UseDirectory("Snapshots");
    }
}