using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Tanka.GraphQL.Server.SourceGenerators.Tests;

public static class TestHelper<TGenerator> where TGenerator : IIncrementalGenerator, new()
{
    public static Task Verify(string source, [CallerFilePath] string sourceFile = "")
    {
        source = new StringBuilder()
            .AppendLine("#nullable enable")
            .AppendLine(source)
            .AppendLine("#nullable restore")
            .ToString();
        
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp12));

        var assemblyReferences = typeof(TGenerator).Assembly
            .GetReferencedAssemblies()
            .ToList();

        IEnumerable<PortableExecutableReference> references = assemblyReferences
            .Select(Assembly.Load)
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Concat(new[]
            {
                MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location)
            })
            .ToList();

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: $"{typeof(TGenerator).Name}.GeneratedSources",
            syntaxTrees: new[] { syntaxTree },
            references: references,
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable));

        var generator = new TGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGenerators(compilation);
        var diagnostics = driver.GetRunResult().Diagnostics;
        return Verifier
            // ReSharper disable once ExplicitCallerInfoArgument
            .Verify(driver, sourceFile: sourceFile)
            .UseDirectory("Snapshots");
    }
}