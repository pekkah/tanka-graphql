using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NSubstitute;
using Tanka.GraphQL.Server.SourceGenerators;
using Xunit;

namespace Tanka.GraphQL.Server.SourceGenerators.Tests;

public class ErrorHandlingFacts
{
    [Fact]
    public async Task ObjectTypeGenerator_ShouldHandleInvalidAttributeUsage()
    {
        // Given
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public interface IQuery  // Interface not supported
                     {
                        string Test { get; }
                     }
                     """;

        // When
        var result = await TestHelper<ObjectTypeGenerator>.Compile(source);

        // Then
        Assert.True(result.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error));
    }

    [Fact]
    public async Task ObjectTypeGenerator_ShouldHandleInvalidMethodSignature()
    {
        // Given
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static void InvalidMethod()  // Void return type not supported
                        {
                        }
                     }
                     """;

        // When
        var result = await TestHelper<ObjectTypeGenerator>.Compile(source);

        // Then
        Assert.True(result.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error));
    }

    [Fact]
    public async Task ObjectTypeGenerator_ShouldHandleGenericTypeWithoutConstraints()
    {
        // Given
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static T GenericMethod<T>()  // Generic methods not supported
                        {
                            return default(T);
                        }
                     }
                     """;

        // When
        var result = await TestHelper<ObjectTypeGenerator>.Compile(source);

        // Then
        Assert.True(result.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error));
    }

    [Fact]
    public async Task ObjectTypeGenerator_ShouldHandleCircularReferences()
    {
        // Given
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static Person GetPerson() => new Person();
                     }

                     [ObjectType]
                     public class Person
                     {
                        public string Name { get; set; }
                        public Person Parent { get; set; }  // Circular reference
                     }
                     """;

        // When
        var result = await TestHelper<ObjectTypeGenerator>.Compile(source);

        // Then
        Assert.False(result.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error));
    }

    [Fact]
    public async Task ObjectTypeGenerator_ShouldHandleInvalidGraphQLName()
    {
        // Given
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     [GraphQLName("123InvalidName")]  // Invalid GraphQL name
                     public static class Query
                     {
                        public static string Test() => "test";
                     }
                     """;

        // When
        var result = await TestHelper<ObjectTypeGenerator>.Compile(source);

        // Then
        Assert.True(result.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error));
    }

    [Fact]
    public async Task ObjectTypeGenerator_ShouldHandleComplexInheritanceHierarchy()
    {
        // Given
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static DerivedClass GetDerived() => new DerivedClass();
                     }

                     public abstract class BaseClass
                     {
                        public abstract string AbstractProperty { get; }
                     }

                     public class MiddleClass : BaseClass
                     {
                        public override string AbstractProperty => "middle";
                        public virtual string VirtualProperty => "virtual";
                     }

                     [ObjectType]
                     public class DerivedClass : MiddleClass
                     {
                        public override string VirtualProperty => "derived";
                        public string NewProperty => "new";
                     }
                     """;

        // When
        var result = await TestHelper<ObjectTypeGenerator>.Compile(source);

        // Then
        Assert.False(result.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error));
    }

    [Fact]
    public async Task InputTypeGenerator_ShouldHandleReadOnlyProperties()
    {
        // Given
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InputType]
                     public class UserInput
                     {
                        public string Name { get; }  // Read-only property
                        public int Age { get; set; }
                     }
                     """;

        // When
        var result = await TestHelper<InputTypeGenerator>.Compile(source);

        // Then
        Assert.True(result.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Warning));
    }

    [Fact]
    public async Task InputTypeGenerator_ShouldHandleInvalidInputType()
    {
        // Given
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InputType]
                     public class UserInput
                     {
                        public Action InvalidAction { get; set; }  // Invalid type for input
                     }
                     """;

        // When
        var result = await TestHelper<InputTypeGenerator>.Compile(source);

        // Then
        Assert.True(result.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error));
    }

    [Fact]
    public async Task TypeHelper_ShouldHandleUnknownSpecialType()
    {
        // Given
        var typeSymbol = Substitute.For<ITypeSymbol>();
        typeSymbol.SpecialType.Returns(SpecialType.None);
        typeSymbol.Name.Returns("UnknownType");

        // When
        var result = TypeHelper.GetGraphQLTypeName(typeSymbol);

        // Then
        Assert.Equal("UnknownType", result);
    }

    [Fact]
    public async Task TypeHelper_ShouldHandleNullableReferenceTypes()
    {
        // Given
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static string? NullableString() => null;
                        public static string NonNullableString() => "test";
                     }
                     """;

        // When
        var result = await TestHelper<ObjectTypeGenerator>.Compile(source);

        // Then
        Assert.False(result.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error));
    }

    [Fact]
    public void TypeHelper_ShouldHandleArrayTypes()
    {
        // Given
        var elementType = Substitute.For<ITypeSymbol>();
        elementType.SpecialType.Returns(SpecialType.System_String);
        elementType.Name.Returns("String");

        var arrayType = Substitute.For<IArrayTypeSymbol>();
        arrayType.ElementType.Returns(elementType);
        arrayType.SpecialType.Returns(SpecialType.None);

        // When
        var result = TypeHelper.GetGraphQLTypeName(arrayType);

        // Then
        Assert.Equal("[String!]", result);
    }

    [Fact]
    public void TypeHelper_ShouldHandleGenericIEnumerable()
    {
        // Given
        var elementType = Substitute.For<ITypeSymbol>();
        elementType.SpecialType.Returns(SpecialType.System_Int32);
        elementType.Name.Returns("Int");

        var genericInterface = Substitute.For<INamedTypeSymbol>();
        genericInterface.OriginalDefinition.SpecialType.Returns(SpecialType.System_Collections_Generic_IEnumerable_T);
        genericInterface.TypeArguments.Returns(new[] { elementType });

        var namedType = Substitute.For<INamedTypeSymbol>();
        namedType.AllInterfaces.Returns(new[] { genericInterface });
        namedType.SpecialType.Returns(SpecialType.None);
        namedType.Name.Returns("List");

        // When
        var result = TypeHelper.GetGraphQLTypeName(namedType);

        // Then
        Assert.Equal("[Int!]", result);
    }

    [Fact]
    public void TypeHelper_ShouldHandleAsyncEnumerable()
    {
        // Given
        var elementType = Substitute.For<ITypeSymbol>();
        elementType.SpecialType.Returns(SpecialType.System_String);
        elementType.Name.Returns("String");

        var asyncEnumerable = Substitute.For<INamedTypeSymbol>();
        asyncEnumerable.IsGenericType.Returns(true);
        asyncEnumerable.ConstructedFrom.Name.Returns("IAsyncEnumerable");
        asyncEnumerable.TypeArguments.Returns(new[] { elementType });

        // When
        var result = TypeHelper.GetGraphQLTypeName(asyncEnumerable);

        // Then
        Assert.Equal("String!", result);
    }

    [Fact]
    public void TypeHelper_ShouldHandleNestedGenericTypes()
    {
        // Given
        var innerType = Substitute.For<ITypeSymbol>();
        innerType.SpecialType.Returns(SpecialType.System_String);
        innerType.Name.Returns("String");

        var listType = Substitute.For<INamedTypeSymbol>();
        listType.IsGenericType.Returns(true);
        listType.ConstructedFrom.Name.Returns("List");
        listType.TypeArguments.Returns(new[] { innerType });

        var genericInterface = Substitute.For<INamedTypeSymbol>();
        genericInterface.OriginalDefinition.SpecialType.Returns(SpecialType.System_Collections_Generic_IEnumerable_T);
        genericInterface.TypeArguments.Returns(new[] { listType });

        var outerType = Substitute.For<INamedTypeSymbol>();
        outerType.AllInterfaces.Returns(new[] { genericInterface });
        outerType.SpecialType.Returns(SpecialType.None);
        outerType.Name.Returns("NestedList");

        // When
        var result = TypeHelper.GetGraphQLTypeName(outerType);

        // Then
        Assert.Equal("[String!]", result);
    }

    // Helper method to get compilation result
    private static class TestHelper<T> where T : class
    {
        public static async Task<CompilationResult> Compile(string source)
        {
            var compilation = CSharpCompilation.Create("TestAssembly")
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                .AddSyntaxTrees(CSharpSyntaxTree.ParseText(source));

            var diagnostics = compilation.GetDiagnostics();
            return new CompilationResult(compilation, diagnostics);
        }
    }

    private record CompilationResult(CSharpCompilation Compilation, IEnumerable<Diagnostic> Diagnostics);
}