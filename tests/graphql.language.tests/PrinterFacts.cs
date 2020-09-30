using System;
using System.Buffers.Text;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Tanka.GraphQL.Language.Internal;
using Tanka.GraphQL.Language.Nodes;
using Xunit;
using Type = System.Type;

namespace Tanka.GraphQL.Language.Tests
{
    public class PrinterFacts
    {
        private void AssertPrinterEquals(string expected, string actual)
        {
            string Normalize(string str)
            {
                str = str
                    .Replace("\r", string.Empty)
                    .Replace("\n", string.Empty);
                
                return Regex.Replace(str, @"\s+", "");
            }

            Assert.Equal(
                Normalize(expected), 
                Normalize(actual), 
                ignoreCase: false, 
                ignoreLineEndingDifferences: true,
                ignoreWhiteSpaceDifferences: true
                );
        }
        
        [Fact]
        public void ExecutableDocument()
        {
            /* Given */
            var source = @"query {
                                field
                            }
                            mutation {
                                field
                            }
                            subscription {
                                field
                            }
                            ";

            var node = Parser.Create(source)
                .ParseExecutableDocument();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void Document_FragmentDefinition()
        {
            /* Given */
            var source = @"fragment address on Person { field }";

            var node = Parser.Create(source)
                .ParseExecutableDocument();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void OperationDefinition_Empty()
        {
            /* Given */
            var source = "query { }";

            var node = Parser.Create(source)
                .ParseOperationDefinition(OperationType.Query);
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void OperationDefinition_Short_Empty()
        {
            /* Given */
            var source = "{ }";

            var node = Parser.Create(source)
                .ParseShortOperationDefinition();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void OperationDefinition_Short_Selection()
        {
            /* Given */
            var source = @"{ 
                            field
                            field2 {
                                ... on Human {
                                    name
                                }
                            }
                           }";

            var node = Parser.Create(source)
                .ParseShortOperationDefinition();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void OperationDefinition_SelectionSet_FieldSelection()
        {
            /* Given */
            var source = "query { field }";

            var node = Parser.Create(source)
                .ParseOperationDefinition(OperationType.Query);
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void OperationDefinition_SelectionSet_InlineFragment()
        {
            /* Given */
            var source = @"query {  
                            ... on Human {
                                field
                            }
                         }";

            var node = Parser.Create(source)
                .ParseOperationDefinition(OperationType.Query);
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void OperationDefinition_SelectionSet_FragmentSpread()
        {
            /* Given */
            var source = @"query {  
                            ...fragmentName
                         }";

            var node = Parser.Create(source)
                .ParseOperationDefinition(OperationType.Query);
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void OperationDefinition_With_Comment_Before()
        {
            /* Given */
            var source = 
                    @"# comment 
                    query { 
                        field 
                    }";

            var node = Parser.Create(source)
                .ParseOperationDefinition(OperationType.Query);
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void OperationDefinition_With_Comment_Before_Selection()
        {
            /* Given */
            var source = 
                @"query {
                        # comment 
                        field 
                    }";

            var node = Parser.Create(source)
                .ParseOperationDefinition(OperationType.Query);
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void OperationDefinition_With_Comment_After_Selection()
        {
            /* Given */
            var source = 
                @"query {
                        field 
                        # comment 
                    }";

            var node = Parser.Create(source)
                .ParseOperationDefinition(OperationType.Query);
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void OperationDefinition_With_Comment_Between_Selections()
        {
            /* Given */
            var source = 
                @"query {
                        field1
                        # comment
                        field2
                    }";

            var node = Parser.Create(source)
                .ParseOperationDefinition(OperationType.Query);
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void OperationDefinition_VariableDefinitions()
        {
            /* Given */
            var source = 
                @"query ($name: String!, $version: Float!) {}";

            var node = Parser.Create(source)
                .ParseOperationDefinition(OperationType.Query);
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void OperationDefinition_Directives()
        {
            /* Given */
            var source = 
                @"query @a @b(a: -0, b:-54.0) {}";

            var node = Parser.Create(source)
                .ParseOperationDefinition(OperationType.Query);
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void FragmentDefinition()
        {
            /* Given */
            var source = "fragment name on Human { field }";
            var node = Parser.Create(source)
                .ParseFragmentDefinition();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void FragmentDefinition_TypeCondition()
        {
            /* Given */
            var source = "fragment name on Human { field }";
            var node = Parser.Create(source)
                .ParseFragmentDefinition();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void FragmentDefinition_Directives()
        {
            /* Given */
            var source = "fragment name on Human @a @b { field }";
            var node = Parser.Create(source)
                .ParseFragmentDefinition();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void FragmentDefinition_SelectionSet()
        {
            /* Given */
            var source = "fragment name on Human @a @b { field }";
            var node = Parser.Create(source)
                .ParseFragmentDefinition();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void Directive()
        {
            /* Given */
            var source = "@name";
            var node = Parser.Create(source)
                .ParseDirective();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void Directive_Arguments()
        {
            /* Given */
            var source = "@name(a: 1, b: true, c: null)";
            var node = Parser.Create(source)
                .ParseDirective();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void Directives()
        {
            /* Given */
            var source = "@name(a: 1, b: true, c: null) @version {";
            var nodes = Parser.Create(source)
                .ParseOptionalDirectives();
            
            /* When */
            var actual = Printer.Print(nodes);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Theory]
        [InlineData("name: [1,2,3]", "name", typeof(ListValue))]
        [InlineData("another: -123.123", "another", typeof(FloatValue))]
        public void Argument(string source, string name, Type valueType)
        {
            var node = Parser.Create(source)
                .ParseArgument();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void Arguments()
        {
            /* Given */
            var source = "(arg1: 123, arg2: -32, arg3: $variable)";
            var nodes = Parser.Create(source)
                .ParseOptionalArguments();
            
            /* When */
            var actual = Printer.Print(nodes!);

            /* Then */
            AssertPrinterEquals(source, actual);
        }
        
        [Fact]
        public void FieldSelection()
        {
            /* Given */
            var source = "field";

            var node = Parser.Create(source)
                .ParseFieldSelection();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void FragmentSpread()
        {
            /* Given */
            var source = "...name";

            var node = Parser.Create(source)
                .ParseFragmentSpread();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void FragmentSpread_Directives()
        {
            /* Given */
            var source = "...name @a @b @c";

            var node = Parser.Create(source)
                .ParseFragmentSpread();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void InlineFragment()
        {
            /* Given */
            var source = "... on Human {}";

            var node = Parser.Create(source)
                .ParseInlineFragment();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void InlineFragment_Directives()
        {
            /* Given */
            var source = "... on Human @a @b {}";

            var node = Parser.Create(source)
                .ParseInlineFragment();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void InlineFragment_SelectionSet()
        {
            /* Given */
            var source = "... on Human { field }";

            var node = Parser.Create(source)
                .ParseInlineFragment();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void InlineFragment_NoTypeCondition_SelectionSet()
        {
            /* Given */
            var source = "... { field }";

            var node = Parser.Create(source)
                .ParseInlineFragment();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void FieldSelection_with_Alias()
        {
            /* Given */
            var source = "alias: field";

            var node = Parser.Create(source)
                .ParseFieldSelection();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void FieldSelection_SelectionSet()
        {
            /* Given */
            var source = "field { subField }";

            var node = Parser.Create(source)
                .ParseFieldSelection();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void VariableDefinitions()
        {
            /* Given */
            var source = @"($name: String! = ""tanka"", $Version: Float = 2.0)";

            var nodes = Parser.Create(source)
                .ParseVariableDefinitions();
            
            /* When */
            var actual = Printer.Print(nodes);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Theory]
        [InlineData("$variable: Int", "variable", "Int")]
        [InlineData("$variable2: String", "variable2", "String")]
        public void VariableDefinition(string source, string name, string typeName)
        {
            var node = Parser.Create(source)
                .ParseVariableDefinition();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Theory]
        [InlineData("$variable: Int=123", typeof(IntValue))]
        [InlineData(@"$variable2: String = ""Test""", typeof(StringValue))]
        public void VariableDefinition_DefaultValue(string source, Type expectedDefaultValueType)
        {
            var node = Parser.Create(source)
                .ParseVariableDefinition();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void Type_NamedType()
        {
            /* Given */
            var source = "TypeName";

            var node = Parser.Create(source)
                .ParseType();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void Type_NonNullOf_NamedType()
        {
            /* Given */
            var source = "TypeName!";

            var node = Parser.Create(source)
                .ParseType();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void Type_ListOf_NamedType()
        {
            /* Given */
            var source = "[TypeName]";

            var node = Parser.Create(source)
                .ParseType();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void Type_NonNullOf_ListOf_NamedType()
        {
            /* Given */
            var source = "[TypeName]!";

            var node = Parser.Create(source)
                .ParseType();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void Type_ListOf_NonNullOf_NamedType()
        {
            /* Given */
            var source = "[TypeName!]";

            var node = Parser.Create(source)
                .ParseType();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Theory]
        [InlineData("123", 123)]
        [InlineData("-123", -123)]
        public void Value_Int(string source, int expected)
        {
            var node = Parser.Create(source)
                .ParseValue();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Theory]
        [InlineData("123.123", 123.123)]
        [InlineData("-123.123", -123.123)]
        [InlineData("123e20", 123e20)]
        public void Value_Float(string source, double expected)
        {
            var node = Parser.Create(source)
                .ParseValue();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Theory]
        [InlineData("\"test\"", "test")]
        [InlineData("\"test test\"", "test test")]
        [InlineData("\"test_test\"", "test_test")]
        public void Value_String(string source, string expected)
        {
            var node = Parser.Create(source)
                .ParseValue();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Theory]
        [InlineData(@"""""""
                     Cat
                       - not a dog
                       - not a goat

                     Might be part demon.

                     """"""", 
            "Cat\n  - not a dog\n  - not a goat\n\nMight be part demon.")]
        public void Value_BlockString(string source, string expected)
        {
            var node = Parser.Create(source)
                .ParseValue();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            var parsedSource = Encoding.UTF8.GetString(new BlockStringValueReader(
                Encoding.UTF8.GetBytes(source))
                .Read());
            
            AssertPrinterEquals(parsedSource, actual);
        }

        [Fact]
        public void Value_BlockString_AsDescription()
        {
            /* Given */
            var source = @"
""""""
Description
multiple lines
""""""
";
            var node = Parser.Create(source)
                .ParseOptionalDescription();
            
            /* When */
            var actual = Printer.Print(node!);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void Value_Null()
        {
            /* Given */
            var source = "null";
            var node = Parser.Create(source)
                .ParseValue();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Theory]
        [InlineData("true", true)]
        [InlineData("false", false)]
        public void Value_BooleanValue(string source, bool expected)
        {
            var node = Parser.Create(source)
                .ParseValue();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Theory]
        [InlineData("ONE", "ONE")]
        [InlineData("TWO", "TWO")]
        [InlineData("ZERO", "ZERO")]
        public void Value_EnumValue(string source, string expected)
        {
            var node = Parser.Create(source)
                .ParseValue();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void Value_ListValue_Empty()
        {
            /* Given */
            var source = "[]";
            var node = Parser.Create(source)
                .ParseValue();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void Value_ListValue_with_IntValues()
        {
            /* Given */
            var source = "[1,2,3]";
            var node = Parser.Create(source)
                .ParseValue();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }
        
        [Theory]
        [InlineData("[1,2,3]")]
        [InlineData("[1.1,2.1,3.1]")]
        [InlineData("[\"1\",\"2\",\"3\"]")]
        public void Value_ListValue_with_Values(string source)
        {
            /* Given */
            var node = Parser.Create(source)
                .ParseValue();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void Value_ObjectValue_Empty()
        {
            /* Given */
            var source = "{}";
            var node = Parser.Create(source)
                .ParseValue();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Fact]
        public void Value_ObjectValue_with_Fields()
        {
            /* Given */
            var source = @"{ name:""tanka"", version: 2.0 }";
            var node = Parser.Create(source)
                .ParseValue();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }

        [Theory]
        [InlineData("{name: 1.0}", "name", typeof(FloatValue))]
        [InlineData(@"{x: ""string""}", "x", typeof(StringValue))]
        [InlineData(@"{empty: null}", "empty", typeof(NullValue))]
        [InlineData(@"{list: [1,2,3]}", "list", typeof(ListValue))]
        public void Value_ObjectValue_ObjectField(string source, string expectedName, Type typeOf)
        {
            var node = Parser.Create(source)
                .ParseObjectValue();
            
            /* When */
            var actual = Printer.Print(node);

            /* Then */
            AssertPrinterEquals(source, actual);
        }
    }
}