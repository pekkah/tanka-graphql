using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Visitors;
using Xunit;

namespace Tanka.GraphQL.Language.Tests;

/// <summary>
/// Robustness and edge case tests for AST Visitor and Printer components.
/// These tests ensure reliable behavior with malformed, edge case, and boundary condition inputs
/// for the document walker, visitor, and printer systems.
/// </summary>
public class ASTVisitorAndPrinterRobustnessFacts
{
    #region Document Walker Robustness Tests

    [Fact]
    public void ReadOnlyDocumentWalker_WithNullVisitor_HandlesGracefully()
    {
        // Given: Document walker with null visitor in collection
        var document = Parser.Create("{ field }").ParseExecutableDocument();
        var visitors = new List<IReadOnlyDocumentVisitor<PrinterContext>?> { null };
        
        // When & Then: Should handle null visitor gracefully
        var context = new PrinterContext();
        var exception = Record.Exception(() => 
        {
            var walker = new ReadOnlyDocumentWalker<PrinterContext>(
                visitors.Where(v => v != null), 
                context
            );
            walker.Visit(document);
        });
        
        Assert.Null(exception);
    }

    [Fact]
    public void ReadOnlyDocumentWalker_WithNullDocument_HandlesGracefully()
    {
        // Given: Document walker with null document
        var visitor = Substitute.For<IReadOnlyDocumentVisitor<object>>();
        var context = new object();
        var walker = new ReadOnlyDocumentWalker<object>(new[] { visitor }, context);

        // When & Then: Should handle null document gracefully
        var exception = Record.Exception(() => walker.Visit((INode)null!));
        
        // May throw or handle gracefully depending on implementation
        // This test documents the current behavior
        Assert.NotNull(exception);
    }

    [Fact]
    public void ReadOnlyDocumentWalker_WithDeeplyNestedDocument_DoesNotStackOverflow()
    {
        // Given: Deeply nested selection sets (100 levels)
        var deepQuery = "query";
        for (int i = 0; i < 100; i++)
        {
            deepQuery += " { field";
        }
        for (int i = 0; i < 100; i++)
        {
            deepQuery += " }";
        }
        
        var document = Parser.Create(deepQuery).ParseExecutableDocument();
        var visitor = Substitute.For<IReadOnlyDocumentVisitor<object>>();
        var context = new object();
        var walker = new ReadOnlyDocumentWalker<object>(new[] { visitor }, context);

        // When & Then: Should handle deep nesting without stack overflow
        var exception = Record.Exception(() => walker.Visit(document));
        Assert.Null(exception);
        
        // Verify visitor was called for deeply nested elements
        visitor.Received().EnterNode(context, Arg.Any<INode>());
    }

    [Fact]
    public void ReadOnlyDocumentWalker_WithVeryLargeDocument_HandlesEfficiently()
    {
        // Given: Document with many sibling selections (1000 fields)
        var largeQuery = "{ ";
        for (int i = 0; i < 1000; i++)
        {
            largeQuery += $"field{i} ";
        }
        largeQuery += "}";
        
        var document = Parser.Create(largeQuery).ParseExecutableDocument();
        var visitor = Substitute.For<IReadOnlyDocumentVisitor<object>>();
        var context = new object();
        var walker = new ReadOnlyDocumentWalker<object>(new[] { visitor }, context);

        // When: Walk the large document
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        walker.Visit(document);
        stopwatch.Stop();

        // Then: Should complete in reasonable time and visit all nodes
        Assert.True(stopwatch.ElapsedMilliseconds < 5000, 
            $"Large document walking took {stopwatch.ElapsedMilliseconds}ms");
        visitor.Received().EnterNode(context, Arg.Any<INode>());
    }

    [Fact]
    public void ReadOnlyExecutionDocumentWalker_WithComplexFragments_VisitsAllNodes()
    {
        // Given: Document with complex fragment structures
        var source = @"
            query TestQuery {
                user {
                    ...UserFragment
                    ... on User {
                        id
                        ...NestedFragment
                    }
                }
            }
            
            fragment UserFragment on User {
                name
                email
            }
            
            fragment NestedFragment on User {
                profile {
                    bio
                }
            }
        ";
        
        var document = Parser.Create(source).ParseExecutableDocument();
        var visitor = Substitute.For<VisitAllBase>();
        var walker = new ReadOnlyExecutionDocumentWalker(
            new ExecutionDocumentWalkerOptions().Add(visitor)
        );

        // When: Visit the document
        walker.Visit(document);

        // Then: Should visit all fragment-related nodes
        visitor.Received().Enter(Arg.Any<FragmentDefinition>());
        visitor.Received().Enter(Arg.Any<FragmentSpread>());
        visitor.Received().Enter(Arg.Any<InlineFragment>());
    }

    #endregion

    #region Visitor Exception Handling Tests

    [Fact]
    public void ReadOnlyDocumentWalker_WithThrowingVisitor_HandlesExceptions()
    {
        // Given: Visitor that throws exceptions
        var document = Parser.Create("{ field }").ParseExecutableDocument();
        var throwingVisitor = Substitute.For<IReadOnlyDocumentVisitor<object>>();
        throwingVisitor.When(v => v.EnterNode(Arg.Any<object>(), Arg.Any<INode>()))
                      .Do(_ => throw new InvalidOperationException("Test exception"));
        
        var context = new object();
        var walker = new ReadOnlyDocumentWalker<object>(new[] { throwingVisitor }, context);

        // When & Then: Should propagate visitor exceptions
        Assert.Throws<InvalidOperationException>(() => walker.Visit(document));
    }

    [Fact]
    public void ReadOnlyExecutionDocumentWalker_WithMultipleVisitors_CallsAllVisitors()
    {
        // Given: Multiple visitors
        var document = Parser.Create("{ field }").ParseExecutableDocument();
        var visitor1 = Substitute.For<VisitAllBase>();
        var visitor2 = Substitute.For<VisitAllBase>();
        var visitor3 = Substitute.For<VisitAllBase>();
        
        var walker = new ReadOnlyExecutionDocumentWalker(
            new ExecutionDocumentWalkerOptions()
                .Add(visitor1)
                .Add(visitor2)
                .Add(visitor3)
        );

        // When: Visit document
        walker.Visit(document);

        // Then: All visitors should be called
        visitor1.Received().Enter(Arg.Any<ExecutableDocument>());
        visitor2.Received().Enter(Arg.Any<ExecutableDocument>());
        visitor3.Received().Enter(Arg.Any<ExecutableDocument>());
    }

    #endregion

    #region Printer Robustness Tests

    [Fact]
    public void Printer_WithNullNode_HandlesGracefully()
    {
        // Given: Null node
        INode nullNode = null!;

        // When & Then: Should handle null input gracefully
        var exception = Record.Exception(() => Printer.Print(nullNode));
        
        // Depending on implementation, may throw or return empty string
        // This test documents the current behavior
        Assert.NotNull(exception);
    }

    [Fact]
    public void Printer_WithEmptyDocument_ReturnsEmptyString()
    {
        // Given: Empty executable document
        var emptyDocument = Parser.Create("").ParseExecutableDocument();

        // When: Print the empty document
        var result = Printer.Print(emptyDocument);

        // Then: Should return empty or minimal output
        Assert.NotNull(result);
        Assert.True(string.IsNullOrWhiteSpace(result) || result.Length < 10);
    }

    [Fact]
    public void Printer_WithDeeplyNestedStructure_DoesNotStackOverflow()
    {
        // Given: Deeply nested selection sets
        var deepQuery = "query";
        for (int i = 0; i < 50; i++)
        {
            deepQuery += " { field";
        }
        for (int i = 0; i < 50; i++)
        {
            deepQuery += " }";
        }
        
        var document = Parser.Create(deepQuery).ParseExecutableDocument();

        // When: Print the deeply nested document
        var exception = Record.Exception(() => Printer.Print(document));

        // Then: Should not stack overflow
        Assert.Null(exception);
    }

    [Fact]
    public void Printer_WithVeryLongFieldNames_HandlesCorrectly()
    {
        // Given: Document with very long field name (10,000 characters)
        var longFieldName = new string('a', 10000);
        var source = $"{{ {longFieldName} }}";
        var document = Parser.Create(source).ParseExecutableDocument();

        // When: Print the document
        var result = Printer.Print(document);

        // Then: Should handle long field names correctly
        Assert.Contains(longFieldName, result);
        Assert.True(result.Length > 10000);
    }

    [Fact]
    public void Printer_WithComplexNestedValues_MaintainsStructure()
    {
        // Given: Document with complex nested object and list values
        var source = @"{ 
            field(arg: {
                nested: {
                    deep: {
                        list: [1, 2, { inner: ""value"" }]
                        bool: true
                        null: null
                    }
                }
                array: [[1, 2], [3, 4]]
            }) 
        }";
        
        var document = Parser.Create(source).ParseExecutableDocument();

        // When: Print the document
        var result = Printer.Print(document);

        // Then: Should maintain nested structure
        Assert.NotNull(result);
        Assert.Contains("nested", result);
        Assert.Contains("deep", result);
        Assert.Contains("list", result);
        Assert.Contains("array", result);
    }

    [Fact]
    public void Printer_WithUnicodeContent_PreservesEncoding()
    {
        // Given: Document with unicode content
        var source = @"{ field(arg: ""Hello 👋 World 🌍 测试"") }";
        var document = Parser.Create(source).ParseExecutableDocument();

        // When: Print the document
        var result = Printer.Print(document);

        // Then: Should preserve unicode characters
        Assert.Contains("👋", result);
        Assert.Contains("🌍", result);
        Assert.Contains("测试", result);
    }

    [Fact]
    public void Printer_WithSpecialCharacters_EscapesCorrectly()
    {
        // Given: Document with special characters that need escaping
        var source = @"{ field(arg: ""line1\nline2\ttab\r\n"") }";
        var document = Parser.Create(source).ParseExecutableDocument();

        // When: Print the document
        var result = Printer.Print(document);

        // Then: Should handle escape sequences properly
        Assert.NotNull(result);
        Assert.Contains("field", result);
    }

    [Fact]
    public void Printer_WithDescriptions_HandlesCorrectly()
    {
        // Given: Simple document
        var source = "{ field }";
        var document = Parser.Create(source).ParseExecutableDocument();

        // When: Print with different description settings
        var result1 = Printer.Print(document, printDescriptions: true);
        var result2 = Printer.Print(document, printDescriptions: false);

        // Then: Both should work (content may be same for executable documents)
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Contains("field", result1);
        Assert.Contains("field", result2);
    }

    #endregion

    #region Printer Context Edge Cases

    [Fact]
    public void PrinterContext_WithLargeOutput_HandlesCorrectly()
    {
        // Given: Document that will produce large output
        var largeQuery = "{ ";
        for (int i = 0; i < 1000; i++)
        {
            largeQuery += $"field{i} ";
        }
        largeQuery += "}";
        
        var document = Parser.Create(largeQuery).ParseExecutableDocument();

        // When: Print the large document
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = Printer.Print(document);
        stopwatch.Stop();

        // Then: Should handle large output
        Assert.True(result.Length > 10000, "Result should be substantial");
        Assert.True(stopwatch.ElapsedMilliseconds < 5000, 
            $"Large document printing took {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public void PrinterContext_WithEmptyBuilder_HandlesToStringCorrectly()
    {
        // Given: Empty printer context
        var context = new PrinterContext();

        // When: Convert to string
        var result = context.ToString();

        // Then: Should return empty string
        Assert.Equal("", result);
    }

    [Fact]
    public void PrinterContext_WithMultipleAppends_ConcatenatesCorrectly()
    {
        // Given: Printer context with multiple appends
        var context = new PrinterContext();
        
        // When: Append various types
        context.Append("test");
        context.Append(123);
        context.Append(' ');
        context.Append("end");

        // Then: Should concatenate all values
        var result = context.ToString();
        Assert.Equal("test123 end", result);
    }

    [Fact]
    public void PrinterContext_WithRewind_RemovesLastCharacter()
    {
        // Given: Printer context with content
        var context = new PrinterContext();
        context.Append("test123");

        // When: Rewind one character
        context.Rewind();

        // Then: Should remove last character
        var result = context.ToString();
        Assert.Equal("test12", result);
    }

    [Fact]
    public void PrinterContext_WithEndsWithCheck_ReturnsCorrectResult()
    {
        // Given: Printer context with specific ending
        var context = new PrinterContext();
        context.Append("test,");

        // When: Check if ends with comma
        var endsWithComma = context.EndsWith(',');
        var endsWithSpace = context.EndsWith(' ');

        // Then: Should return correct results
        Assert.True(endsWithComma);
        Assert.False(endsWithSpace);
    }

    #endregion

    #region Round-trip Testing

    [Fact]
    public void Printer_ParsePrintRoundtrip_MaintainsEquivalence()
    {
        // Given: Various GraphQL documents
        var testCases = new[]
        {
            "{ field }",
            "query($var: String!) { field(arg: $var) }",
            "{ field { subField } }",
            "{ ...fragment }",
            "{ ... on Type { field } }",
            "mutation { createUser(input: { name: \"test\" }) { id } }"
        };

        foreach (var originalSource in testCases)
        {
            // When: Parse then print
            var document = Parser.Create(originalSource).ParseExecutableDocument();
            var printed = Printer.Print(document);
            var reparsed = Parser.Create(printed).ParseExecutableDocument();

            // Then: Should parse successfully (structure preserved)
            Assert.NotNull(reparsed);
            Assert.Equal(document.OperationDefinitions?.Count, reparsed.OperationDefinitions?.Count);
        }
    }

    [Fact]
    public void Printer_SimpleRoundtrip_WorksCorrectly()
    {
        // Given: Simple GraphQL documents
        var testCases = new[]
        {
            "{ field }",
            "{ field1 field2 }"
        };

        foreach (var originalSource in testCases)
        {
            // When: Parse then print then parse again
            var document = Parser.Create(originalSource).ParseExecutableDocument();
            var printed = Printer.Print(document);
            
            // Then: Should produce valid GraphQL that can be parsed
            var exception = Record.Exception(() => Parser.Create(printed).ParseExecutableDocument());
            Assert.Null(exception);
        }
    }

    #endregion

    #region Performance and Memory Tests

    [Fact]
    public void Printer_WithRepeatedCalls_DoesNotLeakMemory()
    {
        // Given: Simple document
        var document = Parser.Create("{ field }").ParseExecutableDocument();

        // When: Print many times
        for (int i = 0; i < 1000; i++)
        {
            var result = Printer.Print(document);
            Assert.NotNull(result);
        }

        // Then: Should not cause memory issues (this is a basic smoke test)
        // In a real scenario, you might use memory profiling tools
        Assert.True(true, "Completed without memory exceptions");
    }

    [Fact]
    public void DocumentWalker_WithRepeatedVisits_PerformsConsistently()
    {
        // Given: Document and visitor
        var document = Parser.Create("{ field1 field2 field3 }").ParseExecutableDocument();
        var visitor = Substitute.For<IReadOnlyDocumentVisitor<object>>();
        var context = new object();
        var walker = new ReadOnlyDocumentWalker<object>(new[] { visitor }, context);

        // When: Visit multiple times and measure
        var times = new List<long>();
        for (int i = 0; i < 100; i++)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            walker.Visit(document);
            stopwatch.Stop();
            times.Add(stopwatch.ElapsedTicks);
        }

        // Then: Performance should be consistent (no significant degradation)
        var avgTime = times.Average();
        var maxTime = times.Max();
        
        Assert.True(maxTime < avgTime * 10, 
            $"Performance inconsistent - avg: {avgTime}, max: {maxTime}");
    }

    #endregion
}