using System;
using System.Buffers.Text;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Tanka.GraphQL.Language.Nodes;
using Xunit;
using Type = System.Type;

namespace Tanka.GraphQL.Language.Tests
{
    public class ParserFacts
    {
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

            var sut = Parser.Create(source);
            
            /* When */
            var actual = sut.ParseExecutableDocument();

            /* Then */
            Assert.Equal(3, actual.OperationDefinitions?.Count);
        }

        [Fact]
        public void Document_FragmentDefinition()
        {
            /* Given */
            var source = @"fragment address on Person { field }";

            var sut = Parser.Create(source);
            
            /* When */
            var actual = sut.ParseExecutableDocument();

            /* Then */
            Assert.Equal(1, actual.FragmentDefinitions?.Count);
        }

        [Fact]
        public void OperationDefinition_Empty()
        {
            /* Given */
            var source = "query { }";

            var sut = Parser.Create(source);
            
            /* When */
            var actual = sut.ParseOperationDefinition(OperationType.Query);

            /* Then */
            Assert.Equal(OperationType.Query, actual.Operation);
        }

        [Fact]
        public void OperationDefinition_Short_Empty()
        {
            /* Given */
            var source = "{ }";

            var sut = Parser.Create(source);
            
            /* When */
            var actual = sut.ParseShortOperationDefinition();

            /* Then */
            Assert.Equal(OperationType.Query, actual.Operation);
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

            var sut = Parser.Create(source);
            
            /* When */
            var actual = sut.ParseShortOperationDefinition();

            /* Then */
            Assert.Equal(2, actual.SelectionSet.Selections.Count);
        }

        [Fact]
        public void OperationDefinition_SelectionSet_FieldSelection()
        {
            /* Given */
            var source = "query { field }";

            var sut = Parser.Create(source);

            /* When */
            var actual = sut.ParseOperationDefinition(OperationType.Query);

            /* Then */
            Assert.Single(actual.SelectionSet.Selections);
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

            var sut = Parser.Create(source);

            /* When */
            var actual = sut.ParseOperationDefinition(OperationType.Query);

            /* Then */
            Assert.Single(actual.SelectionSet.Selections);
            Assert.IsType<InlineFragment>(actual.SelectionSet.Selections.Single());
        }

        [Fact]
        public void OperationDefinition_SelectionSet_FragmentSpread()
        {
            /* Given */
            var source = @"query {  
                            ...fragmentName
                         }";

            var sut = Parser.Create(source);

            /* When */
            var actual = sut.ParseOperationDefinition(OperationType.Query);

            /* Then */
            Assert.Single(actual.SelectionSet.Selections);
            Assert.IsType<FragmentSpread>(actual.SelectionSet.Selections.Single());
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

            var sut = Parser.Create(source);

            /* When */
            var actual = sut.ParseOperationDefinition(OperationType.Query);

            /* Then */
            Assert.Single(actual.SelectionSet.Selections);
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

            var sut = Parser.Create(source);

            /* When */
            var actual = sut.ParseOperationDefinition(OperationType.Query);

            /* Then */
            Assert.Single(actual.SelectionSet.Selections);
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

            var sut = Parser.Create(source);

            /* When */
            var actual = sut.ParseOperationDefinition(OperationType.Query);

            /* Then */
            Assert.Single(actual.SelectionSet.Selections);
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

            var sut = Parser.Create(source);

            /* When */
            var actual = sut.ParseOperationDefinition(OperationType.Query);

            /* Then */
            Assert.True(actual.SelectionSet.Selections.Count == 2);
        }

        [Fact]
        public void OperationDefinition_VariableDefinitions()
        {
            /* Given */
            var source = 
                @"query ($name: String!, $version: Float!) {}";

            var sut = Parser.Create(source);

            /* When */
            var actual = sut.ParseOperationDefinition(OperationType.Query);

            /* Then */
            Assert.Equal(2, actual.VariableDefinitions?.Count);
        }

        [Fact]
        public void OperationDefinition_Directives()
        {
            /* Given */
            var source = 
                @"query @a @b(a: -0, b:-54.0) {}";

            var sut = Parser.Create(source);

            /* When */
            var actual = sut.ParseOperationDefinition(OperationType.Query);

            /* Then */
            Assert.Equal(2, actual.Directives?.Count);
        }

        [Fact]
        public void FragmentDefinition()
        {
            /* Given */
            var sut = Parser.Create("fragment name on Human { field }");

            /* When */
            var fragmentDefinition = sut.ParseFragmentDefinition();

            /* Then */
            Assert.Equal("name", fragmentDefinition.FragmentName);
        }

        [Fact]
        public void FragmentDefinition_TypeCondition()
        {
            /* Given */
            var sut = Parser.Create("fragment name on Human { field }");

            /* When */
            var fragmentDefinition = sut.ParseFragmentDefinition();

            /* Then */
            Assert.Equal("Human", fragmentDefinition.TypeCondition.Name);
        }

        [Fact]
        public void FragmentDefinition_Directives()
        {
            /* Given */
            var sut = Parser.Create("fragment name on Human @a @b { field }");

            /* When */
            var fragmentDefinition = sut.ParseFragmentDefinition();

            /* Then */
            Assert.Equal(2, fragmentDefinition.Directives.Count);
        }

        [Fact]
        public void FragmentDefinition_SelectionSet()
        {
            /* Given */
            var sut = Parser.Create("fragment name on Human @a @b { field }");

            /* When */
            var fragmentDefinition = sut.ParseFragmentDefinition();

            /* Then */
            Assert.Equal(1, fragmentDefinition.SelectionSet.Selections.Count);
        }

        [Fact]
        public void Directive()
        {
            /* Given */
            var sut = Parser.Create("@name");

            /* When */
            var directive = sut.ParseDirective();

            /* Then */
            Assert.Equal("name", directive.Name);
        }

        [Fact]
        public void Directive_Arguments()
        {
            /* Given */
            var sut = Parser.Create("@name(a: 1, b: true, c: null)");

            /* When */
            var directive = sut.ParseDirective();

            /* Then */
            Assert.Equal(3, directive.Arguments?.Count);
        }

        [Fact]
        public void Directives()
        {
            /* Given */
            var sut = Parser.Create("@name(a: 1, b: true, c: null) @version {");

            /* When */
            var directives = sut.ParseOptionalDirectives();

            /* Then */
            Assert.Equal(2, directives?.Count);
        }

        [Theory]
        [InlineData("name: [1,2,3]", "name", typeof(ListValue))]
        [InlineData("another: -123.123", "another", typeof(FloatValue))]
        public void Argument(string source, string name, Type valueType)
        {
            /* Given */
            var sut = Parser.Create(source);

            /* When */
            var argument = sut.ParseArgument();

            /* Then */
            Assert.Equal(name, argument.Name);
            Assert.IsType(valueType, argument.Value);
        }

        [Fact]
        public void Arguments()
        {
            /* Given */
            var sut = Parser.Create("(arg1: 123, arg2: -32, arg3: $variable)");

            /* When */
            var arguments = sut.ParseOptionalArguments();

            /* Then */
            Assert.Equal(3, arguments?.Count);
        }
        
        [Fact]
        public void FieldSelection()
        {
            /* Given */
            var source = "field";

            var sut = Parser.Create(source);

            /* When */
            var actual = sut.ParseFieldSelection();

            /* Then */
            Assert.Equal("field", actual.Name);
        }

        [Fact]
        public void FragmentSpread()
        {
            /* Given */
            var source = "...name";

            var sut = Parser.Create(source);

            /* When */
            var actual = sut.ParseFragmentSpread();

            /* Then */
            Assert.Equal("name", actual.FragmentName);
        }

        [Fact]
        public void FragmentSpread_Directives()
        {
            /* Given */
            var source = "...name @a @b @c";

            var sut = Parser.Create(source);

            /* When */
            var actual = sut.ParseFragmentSpread();

            /* Then */
            Assert.Equal(3, actual.Directives?.Count);
        }

        [Fact]
        public void InlineFragment()
        {
            /* Given */
            var source = "... on Human {}";

            var sut = Parser.Create(source);

            /* When */
            var actual = sut.ParseInlineFragment();

            /* Then */
            Assert.Equal("Human", actual.TypeCondition?.Name);
        }

        [Fact]
        public void InlineFragment_Directives()
        {
            /* Given */
            var source = "... on Human @a @b {}";

            var sut = Parser.Create(source);

            /* When */
            var actual = sut.ParseInlineFragment();

            /* Then */
            Assert.Equal(2, actual.Directives?.Count);
        }

        [Fact]
        public void InlineFragment_SelectionSet()
        {
            /* Given */
            var source = "... on Human { field }";

            var sut = Parser.Create(source);

            /* When */
            var actual = sut.ParseInlineFragment();

            /* Then */
            Assert.Equal(1, actual.SelectionSet.Selections.Count);
        }

        [Fact]
        public void InlineFragment_NoTypeCondition_SelectionSet()
        {
            /* Given */
            var source = "... { field }";

            var sut = Parser.Create(source);

            /* When */
            var actual = sut.ParseInlineFragment();

            /* Then */
            Assert.Equal(1, actual.SelectionSet.Selections.Count);
        }

        [Fact]
        public void FieldSelection_with_Alias()
        {
            /* Given */
            var source = "alias: field";

            var sut = Parser.Create(source);

            /* When */
            var actual = sut.ParseFieldSelection();

            /* Then */
            Assert.Equal("alias", actual.Alias);
            Assert.Equal("field", actual.Name);
        }

        [Fact]
        public void FieldSelection_SelectionSet()
        {
            /* Given */
            var source = "field { subField }";

            var sut = Parser.Create(source);

            /* When */
            var actual = sut.ParseFieldSelection();

            /* Then */
            Assert.NotNull(actual.SelectionSet?.Selections);
            Assert.NotEmpty(actual.SelectionSet.Selections);
        }

        [Fact]
        public void VariableDefinitions()
        {
            /* Given */
            var source = @"($name: String! = ""tanka"", $Version: Float = 2.0)";

            var sut = Parser.Create(source);

            /* When */
            var variableDefinitions = sut.ParseVariableDefinitions();

            /* Then */
            Assert.Equal(2, variableDefinitions.Count);
        }

        [Theory]
        [InlineData("$variable: Int", "variable", "Int")]
        [InlineData("$variable2: String", "variable2", "String")]
        public void VariableDefinition(string source, string name, string typeName)
        {
            /* Given */
            var sut = Parser.Create(source);

            /* When */
            var actual = sut.ParseVariableDefinition();

            /* Then */
            Assert.Equal(name, actual.Variable.Name);
            var namedType = Assert.IsType<NamedType>(actual.Type);
            Assert.Equal(typeName, namedType.Name);
        }

        [Theory]
        [InlineData("$variable: Int=123", typeof(IntValue))]
        [InlineData(@"$variable2: String = ""Test""", typeof(StringValue))]
        public void VariableDefinition_DefaultValue(string source, Type expectedDefaultValueType)
        {
            /* Given */
            var sut = Parser.Create(source);

            /* When */
            var actual = sut.ParseVariableDefinition();

            /* Then */
            Assert.IsType(expectedDefaultValueType, actual.DefaultValue?.Value);
        }

        [Fact]
        public void Type_NamedType()
        {
            /* Given */
            var source = "TypeName";

            var sut = Parser.Create(source);

            /* When */
            var type = sut.ParseType();

            /* Then */
            Assert.NotNull(type);
            Assert.IsType<NamedType>(type);
            Assert.Equal("TypeName", ((NamedType)type).Name);
        }

        [Fact]
        public void Type_NonNullOf_NamedType()
        {
            /* Given */
            var source = "TypeName!";

            var sut = Parser.Create(source);

            /* When */
            var type = sut.ParseType();

            /* Then */
            Assert.NotNull(type);
            Assert.IsType<NonNullType>(type);
            Assert.IsType<NamedType>(((NonNullType)type).OfType);
        }

        [Fact]
        public void Type_ListOf_NamedType()
        {
            /* Given */
            var source = "[TypeName]";

            var sut = Parser.Create(source);

            /* When */
            var type = sut.ParseType();

            /* Then */
            Assert.NotNull(type);
            Assert.IsType<ListType>(type);
            Assert.IsType<NamedType>(((ListType)type).OfType);
        }

        [Fact]
        public void Type_NonNullOf_ListOf_NamedType()
        {
            /* Given */
            var source = "[TypeName]!";

            var sut = Parser.Create(source);

            /* When */
            var type = sut.ParseType();

            /* Then */
            Assert.NotNull(type);
            Assert.IsType<NonNullType>(type);
            Assert.IsType<ListType>(((NonNullType)type).OfType);
        }

        [Fact]
        public void Type_ListOf_NonNullOf_NamedType()
        {
            /* Given */
            var source = "[TypeName!]";

            var sut = Parser.Create(source);

            /* When */
            var type = sut.ParseType();

            /* Then */
            Assert.NotNull(type);
            Assert.IsType<ListType>(type);
            Assert.IsType<NonNullType>(((ListType)type).OfType);
        }

        [Fact]
        public void Type_NonNull_ListOf_NonNullOf_NamedType()
        {
            /* Given */
            var source = "[TypeName!]!";

            var sut = Parser.Create(source);

            /* When */
            var type = sut.ParseType();

            /* Then */
            Assert.NotNull(type);
            var nonNullOf = Assert.IsType<NonNullType>(type);
            var listOf = Assert.IsType<ListType>(nonNullOf.OfType);
            var nonNullItemOf = Assert.IsType<NonNullType>(listOf.OfType);
            Assert.IsType<NamedType>(nonNullItemOf.OfType);
        }

        [Theory]
        [InlineData("123", 123)]
        [InlineData("-123", -123)]
        public void Value_Int(string source, int expected)
        {
            /* Given */
            var sut = Parser.Create(source);

            /* When */
            var value = sut.ParseValue();

            /* Then */
            var intValue = Assert.IsType<IntValue>(value);
            Assert.Equal(expected, intValue.Value);
        }

        [Theory]
        [InlineData("123.123", 123.123)]
        [InlineData("-123.123", -123.123)]
        [InlineData("123e20", 123e20)]
        public void Value_Float(string source, double expected)
        {
            /* Given */
            var sut = Parser.Create(source);

            /* When */
            var value = sut.ParseValue();

            /* Then */
            var floatValue = Assert.IsType<FloatValue>(value);
            Assert.True(Utf8Parser.TryParse(floatValue.Value.Span, out double d, out _));
            Assert.Equal(expected, d);
        }

        [Theory]
        [InlineData("\"test\"", "test")]
        [InlineData("\"test test\"", "test test")]
        [InlineData("\"test_test\"", "test_test")]
        public void Value_String(string source, string expected)
        {
            /* Given */
            var sut = Parser.Create(source);

            /* When */
            var value = sut.ParseValue();

            /* Then */
            var stringValue = Assert.IsType<StringValue>(value);
            Assert.Equal(expected, stringValue);
        }

        [Theory]
        [InlineData("\"\"\"test\"\"\"", "test")]
        [InlineData("\"\"\"test test\"\"\"", "test test")]
        [InlineData("\"\"\"test_test\"\"\"", "test_test")]
        [InlineData(@"""""""
                        test
                      """"""", 
                    "test")]
        [InlineData(@"""""""
                      Cat
                        - not a dog
                        - not a goat

                      Might be part demon.

                      """"""", 
            "Cat\n  - not a dog\n  - not a goat\n\nMight be part demon.")]
        public void Value_BlockString(string source, string expected)
        {
            /* Given */
            var sut = Parser.Create(source);

            /* When */
            var value = sut.ParseValue();

            /* Then */
            var blockStringValue = Assert.IsType<StringValue>(value);
            Assert.Equal(expected, blockStringValue);
        }

        [Fact]
        public void Value_BlockString_AsDescription()
        {
            /* Given */
            var sut = Parser.Create(@"
""""""
Description
""""""
");
            /* When */
            var value = sut.ParseOptionalDescription();

            /* Then */
            Assert.Equal("Description", value);
        }

        [Fact]
        public void Value_Null()
        {
            /* Given */
            var sut = Parser.Create("null");

            /* When */
            var value = sut.ParseValue();

            /* Then */
            Assert.IsType<NullValue>(value);
        }

        [Theory]
        [InlineData("true", true)]
        [InlineData("false", false)]
        [InlineData("True", true)]
        [InlineData("False", false)]
        public void Value_BooleanValue(string source, bool expected)
        {
            /* Given */
            var sut = Parser.Create(source);

            /* When */
            var value = sut.ParseValue();

            /* Then */
            var booleanValue = Assert.IsType<BooleanValue>(value);
            Assert.Equal(expected, booleanValue.Value);
        }

        [Theory]
        [InlineData("ONE", "ONE")]
        [InlineData("TWO", "TWO")]
        [InlineData("ZERO", "ZERO")]
        public void Value_EnumValue(string source, string expected)
        {
            /* Given */
            var sut = Parser.Create(source);

            /* When */
            var value = sut.ParseValue();

            /* Then */
            var enumValue = Assert.IsType<EnumValue>(value);
            Assert.Equal(expected, enumValue.Name);
        }

        [Fact]
        public void Value_ListValue_Empty()
        {
            /* Given */
            var sut = Parser.Create("[]");

            /* When */
            var value = sut.ParseValue();

            /* Then */
            var listValue = Assert.IsType<ListValue>(value);
            Assert.Equal(0, listValue.Values.Count);
        }

        [Fact]
        public void Value_ListValue_with_IntValues()
        {
            /* Given */
            var sut = Parser.Create("[1,2,3]");

            /* When */
            var value = sut.ParseValue();

            /* Then */
            var listValue = Assert.IsType<ListValue>(value);
            Assert.Equal(3, listValue.Values.Count);
            Assert.All(listValue.Values, v => Assert.IsType<IntValue>(v));
        }

        [Fact]
        public void Value_ObjectValue_Empty()
        {
            /* Given */
            var sut = Parser.Create("{}");

            /* When */
            var value = sut.ParseValue();

            /* Then */
            var listValue = Assert.IsType<ObjectValue>(value);
            Assert.Equal(0, listValue.Fields.Count);
        }

        [Fact]
        public void Value_ObjectValue_with_Fields()
        {
            /* Given */
            var sut = Parser.Create(@"{ name:""tanka"", version: 2.0 }");

            /* When */
            var value = sut.ParseValue();

            /* Then */
            var listValue = Assert.IsType<ObjectValue>(value);
            Assert.Equal(2, listValue.Fields.Count);
        }

        [Theory]
        [InlineData("name: 1.0", "name", typeof(FloatValue))]
        [InlineData(@"x: ""string""", "x", typeof(StringValue))]
        [InlineData(@"empty: null", "empty", typeof(NullValue))]
        [InlineData(@"list: [1,2,3]", "list", typeof(ListValue))]
        public void Value_ObjectValue_ObjectField(string source, string expectedName, Type typeOf)
        {
            /* Given */
            var sut = Parser.Create(source);

            /* When */
            var value = sut.ParseObjectField();

            /* Then */
            var field = Assert.IsType<ObjectField>(value);
            Assert.Equal(expectedName, field.Name);
            Assert.IsType(typeOf, field.Value);
        }
    }
}