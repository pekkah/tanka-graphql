using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Tanka.GraphQL.Server.SourceGenerators.Tests;

using Xunit;

namespace Tanka.GraphQL.Server.SourceGenerators.Tests;

public class ErrorHandlingFacts
{
    [Fact]
    public async Task ObjectGenerator_WithInvalidSyntax_ReportsError()
    {
        /* Given */
        var source = @"
            using Tanka.GraphQL.Server;

            namespace Tests;

            [ObjectType]
            public class InvalidSyntaxController
            {
                // Missing closing brace for this class
            ";

        /* When */
        var result = await TestHelper.GetGeneratedOutput(source);

        /* Then */
        Assert.True(result.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains(result.Diagnostics, d => d.Id == "CS1513" || d.Id == "CS1514"); // Expected } or similar
    }

    [Fact]
    public async Task ObjectGenerator_WithMissingAttribute_GeneratesNothing()
    {
        /* Given */
        var source = @"
            using Tanka.GraphQL.Server;

            namespace Tests;

            public class NoAttributeController
            {
                public string GetHello() => ""Hello"";
            }";

        /* When */
        var result = await TestHelper.GetGeneratedOutput(source);

        /* Then */
        Assert.Empty(result.GeneratedSources);
    }

    [Fact]
    public async Task ObjectGenerator_WithInvalidAttributeUsage_ReportsError()
    {
        /* Given */
        var source = @"
            using Tanka.GraphQL.Server;

            namespace Tests;

            [ObjectType]
            public interface IInvalidController
            {
                string GetHello();
            }";

        /* When */
        var result = await TestHelper.GetGeneratedOutput(source);

        /* Then */
        Assert.True(result.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains(result.Diagnostics, d => d.GetMessage().Contains("ObjectType attribute can only be applied to classes"));
    }

    [Fact]
    public async Task ObjectGenerator_WithInvalidMethodReturnType_ReportsError()
    {
        /* Given */
        var source = @"
            using Tanka.GraphQL.Server;

            namespace Tests;

            [ObjectType]
            public class InvalidReturnTypeController
            {
                public void GetHello() // void is not valid GraphQL return type
                {
                }
            }";

        /* When */
        var result = await TestHelper.GetGeneratedOutput(source);

        /* Then */
        Assert.True(result.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains(result.Diagnostics, d => d.GetMessage().Contains("void") || d.GetMessage().Contains("return type"));
    }

    [Fact]
    public async Task ObjectGenerator_WithPrivateClass_ReportsError()
    {
        /* Given */
        var source = @"
            using Tanka.GraphQL.Server;

            namespace Tests;

            [ObjectType]
            private class PrivateController
            {
                public string GetHello() => ""Hello"";
            }";

        /* When */
        var result = await TestHelper.GetGeneratedOutput(source);

        /* Then */
        Assert.True(result.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains(result.Diagnostics, d => d.GetMessage().Contains("private") || d.GetMessage().Contains("accessibility"));
    }

    [Fact]
    public async Task ObjectGenerator_WithNestedClass_ReportsError()
    {
        /* Given */
        var source = @"
            using Tanka.GraphQL.Server;

            namespace Tests;

            public class OuterClass
            {
                [ObjectType]
                public class NestedController
                {
                    public string GetHello() => ""Hello"";
                }
            }";

        /* When */
        var result = await TestHelper.GetGeneratedOutput(source);

        /* Then */
        Assert.True(result.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains(result.Diagnostics, d => d.GetMessage().Contains("nested") || d.GetMessage().Contains("top-level"));
    }

    [Fact]
    public async Task ObjectGenerator_WithGenericClass_ReportsError()
    {
        /* Given */
        var source = @"
            using Tanka.GraphQL.Server;

            namespace Tests;

            [ObjectType]
            public class GenericController<T>
            {
                public string GetHello() => ""Hello"";
            }";

        /* When */
        var result = await TestHelper.GetGeneratedOutput(source);

        /* Then */
        Assert.True(result.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains(result.Diagnostics, d => d.GetMessage().Contains("generic") || d.GetMessage().Contains("type parameter"));
    }

    [Fact]
    public async Task ObjectGenerator_WithInvalidGraphQLName_ReportsError()
    {
        /* Given */
        var source = @"
            using Tanka.GraphQL.Server;

            namespace Tests;

            [ObjectType(""123InvalidName"")] // GraphQL names cannot start with numbers
            public class InvalidNameController
            {
                public string GetHello() => ""Hello"";
            }";

        /* When */
        var result = await TestHelper.GetGeneratedOutput(source);

        /* Then */
        Assert.True(result.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains(result.Diagnostics, d => d.GetMessage().Contains("GraphQL name") || d.GetMessage().Contains("invalid"));
    }

    [Fact]
    public async Task ObjectGenerator_WithComplexGenericReturnType_HandlesCorrectly()
    {
        /* Given */
        var source = @"
            using System;
            using System.Collections.Generic;
            using System.Threading.Tasks;
            using Tanka.GraphQL.Server;

            namespace Tests;

            [ObjectType]
            public class ComplexGenericController
            {
                public Task<List<Dictionary<string, object>>> GetComplexData()
                {
                    return Task.FromResult(new List<Dictionary<string, object>>());
                }
            }";

        /* When */
        var result = await TestHelper.GetGeneratedOutput(source);

        /* Then */
        // Complex generics should either be handled gracefully or report a clear error
        if (result.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
        {
            Assert.Contains(result.Diagnostics, d => d.GetMessage().Contains("complex") || d.GetMessage().Contains("generic"));
        }
        else
        {
            Assert.NotEmpty(result.GeneratedSources);
        }
    }

    [Fact]
    public async Task ObjectGenerator_WithCircularReference_HandlesCorrectly()
    {
        /* Given */
        var source = @"
            using Tanka.GraphQL.Server;

            namespace Tests;

            [ObjectType]
            public class UserController
            {
                public User GetUser() => new User();
            }

            [ObjectType]
            public class User
            {
                public UserController Controller { get; set; }
            }";

        /* When */
        var result = await TestHelper.GetGeneratedOutput(source);

        /* Then */
        // Should either handle circular reference or report error
        if (result.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
        {
            Assert.Contains(result.Diagnostics, d => d.GetMessage().Contains("circular") || d.GetMessage().Contains("reference"));
        }
        else
        {
            Assert.NotEmpty(result.GeneratedSources);
        }
    }

    [Fact]
    public async Task ObjectGenerator_WithInvalidParameterType_ReportsError()
    {
        /* Given */
        var source = @"
            using Tanka.GraphQL.Server;

            namespace Tests;

            [ObjectType]
            public class InvalidParameterController
            {
                public string GetData(System.IO.Stream stream) // Stream is not valid GraphQL input
                {
                    return ""data"";
                }
            }";

        /* When */
        var result = await TestHelper.GetGeneratedOutput(source);

        /* Then */
        Assert.True(result.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains(result.Diagnostics, d => d.GetMessage().Contains("Stream") || d.GetMessage().Contains("parameter"));
    }

    [Fact]
    public async Task ObjectGenerator_WithMissingNamespace_HandlesCorrectly()
    {
        /* Given */
        var source = @"
            using Tanka.GraphQL.Server;

            [ObjectType]
            public class NoNamespaceController
            {
                public string GetHello() => ""Hello"";
            }";

        /* When */
        var result = await TestHelper.GetGeneratedOutput(source);

        /* Then */
        // Should either handle missing namespace or report warning
        if (result.Diagnostics.Any(d => d.Severity >= DiagnosticSeverity.Warning))
        {
            Assert.Contains(result.Diagnostics, d => d.GetMessage().Contains("namespace"));
        }
        else
        {
            Assert.NotEmpty(result.GeneratedSources);
        }
    }

    [Fact]
    public async Task ObjectGenerator_WithDuplicateFieldNames_ReportsError()
    {
        /* Given */
        var source = @"
            using Tanka.GraphQL.Server;

            namespace Tests;

            [ObjectType]
            public class DuplicateFieldController
            {
                public string GetHello() => ""Hello"";
                
                [GraphQLName(""hello"")]
                public string GetGreeting() => ""Greeting"";
            }";

        /* When */
        var result = await TestHelper.GetGeneratedOutput(source);

        /* Then */
        Assert.True(result.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains(result.Diagnostics, d => d.GetMessage().Contains("duplicate") || d.GetMessage().Contains("hello"));
    }

    [Fact]
    public async Task InputTypeGenerator_WithInvalidInputType_ReportsError()
    {
        /* Given */
        var source = @"
            using Tanka.GraphQL.Server;

            namespace Tests;

            [InputType]
            public class InvalidInputType
            {
                public System.IO.Stream Data { get; set; } // Stream is not valid GraphQL input
            }";

        /* When */
        var result = await TestHelper.GetGeneratedOutput(source);

        /* Then */
        Assert.True(result.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains(result.Diagnostics, d => d.GetMessage().Contains("Stream") || d.GetMessage().Contains("input"));
    }

    [Fact]
    public async Task InterfaceGenerator_WithInvalidInterface_ReportsError()
    {
        /* Given */
        var source = @"
            using Tanka.GraphQL.Server;

            namespace Tests;

            [InterfaceType]
            public class NotAnInterface // Should be interface, not class
            {
                public string Name { get; set; }
            }";

        /* When */
        var result = await TestHelper.GetGeneratedOutput(source);

        /* Then */
        Assert.True(result.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains(result.Diagnostics, d => d.GetMessage().Contains("interface") || d.GetMessage().Contains("InterfaceType"));
    }

    [Fact]
    public async Task ObjectGenerator_WithTooManyParameters_ReportsError()
    {
        /* Given */
        var source = @"
            using Tanka.GraphQL.Server;

            namespace Tests;

            [ObjectType]
            public class TooManyParametersController
            {
                public string GetData(
                    string p1, string p2, string p3, string p4, string p5,
                    string p6, string p7, string p8, string p9, string p10,
                    string p11, string p12, string p13, string p14, string p15
                ) // Too many parameters
                {
                    return ""data"";
                }
            }";

        /* When */
        var result = await TestHelper.GetGeneratedOutput(source);

        /* Then */
        // Should handle large number of parameters gracefully
        if (result.Diagnostics.Any(d => d.Severity >= DiagnosticSeverity.Warning))
        {
            Assert.Contains(result.Diagnostics, d => d.GetMessage().Contains("parameter"));
        }
        else
        {
            Assert.NotEmpty(result.GeneratedSources);
        }
    }

    [Fact]
    public async Task ObjectGenerator_WithInvalidAsyncMethod_ReportsError()
    {
        /* Given */
        var source = @"
            using System.Threading.Tasks;
            using Tanka.GraphQL.Server;

            namespace Tests;

            [ObjectType]
            public class InvalidAsyncController
            {
                public async void GetDataAsync() // async void is not valid
                {
                    await Task.Delay(1);
                }
            }";

        /* When */
        var result = await TestHelper.GetGeneratedOutput(source);

        /* Then */
        Assert.True(result.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains(result.Diagnostics, d => d.GetMessage().Contains("async") || d.GetMessage().Contains("void"));
    }
}