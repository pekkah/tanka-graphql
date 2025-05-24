using System;
using System.Buffers.Text;
using System.Linq;
using Tanka.GraphQL.Language.Nodes;
using Xunit;

namespace Tanka.GraphQL.Language.Tests;

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
        Assert.Equal(2, actual.SelectionSet.Count);
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
        Assert.Single(actual.SelectionSet);
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
        Assert.Single(actual.SelectionSet);
        Assert.IsType<InlineFragment>(actual.SelectionSet.Single());
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
        Assert.Single(actual.SelectionSet);
        Assert.IsType<FragmentSpread>(actual.SelectionSet.Single());
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
        Assert.Single(actual.SelectionSet);
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
        Assert.Single(actual.SelectionSet);
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
        Assert.Single(actual.SelectionSet);
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
        Assert.True(actual.SelectionSet.Count == 2);
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
        Assert.Equal(1, fragmentDefinition.SelectionSet.Count);
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
        var sut = Parser.Create("@name(a: 1, b: true, c: null) @version");

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
        Assert.Equal(1, actual.SelectionSet.Count);
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
        Assert.Equal(1, actual.SelectionSet.Count);
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
        Assert.NotNull(actual.SelectionSet);
        Assert.NotEmpty(actual.SelectionSet);
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
    [InlineData("0", 0)]
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
        Assert.Equal(0, listValue.Count);
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
        Assert.Equal(3, listValue.Count);
        Assert.All(listValue, v => Assert.IsType<IntValue>(v));
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
        Assert.Equal(0, listValue.Count);
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
        Assert.Equal(2, listValue.Count);
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

    #region Parser Error Handling Tests
    public class ParserErrorHandlingFacts
    {
        private void AssertParsingThrows(string source, string? expectedMessagePart = null, Type? expectedExceptionType = null)
        {
            var parser = Parser.Create(source);
            var exception = Assert.Throws(expectedExceptionType ?? typeof(Exception), () => parser.ParseExecutableDocument());
            
            if (!string.IsNullOrEmpty(expectedMessagePart))
            {
                Assert.Contains(expectedMessagePart, exception.Message);
            }
        }

        [Theory]
        [InlineData("query { field @ }", "Unexpected token At")] // Unexpected punctuator instead of selection set end or new field
        [InlineData("query name @dir { field }", "Unexpected token At")] // Unexpected directive symbol after operation name, expecting '{' or '('
        [InlineData("fragment name on Type @dir @ { field }", "Unexpected token At")] // Unexpected punctuator after a directive
        [InlineData("query { ... on }", "Unexpected token RightBrace")] // Missing Type for inline fragment
        [InlineData("query { ... }", "Unexpected token RightBrace")] // Missing fragment name or inline type condition
        public void Parse_Throws_On_UnexpectedToken(string source, string expectedMessagePart)
        {
            AssertParsingThrows(source, expectedMessagePart);
        }

        [Theory]
        [InlineData("query { field (arg: ) }", "Expected Value")] // Missing value for an argument
        [InlineData("query name (var: String", "Unexpected token End")] // Missing closing parenthesis for variable definitions
        [InlineData("fragment name on { field }", "Expected Name")] // Missing type name for type condition for fragment
        [InlineData("fragment on Type { field }", "Expected Name for fragment")] // Missing fragment name
        [InlineData("fragment name Type { field }", "Expected Name(on)")] // Missing 'on' keyword
        public void Parse_Throws_On_MissingExpectedToken(string source, string expectedMessagePart)
        {
            AssertParsingThrows(source, expectedMessagePart);
        }

        [Theory]
        // Note: "qyery { field }" would be a lexer error (invalid Name token if 'qyery' not allowed, or just a Name token).
        // Parser expects specific keywords like "query", "mutation", "subscription", "fragment".
        // If "qyery" is lexed as a Name, parser will say "Expected one of [query, mutation, subscription, fragment, {]"
        [InlineData("qyery { field }", "Unexpected token Name(qyery)")] 
        [InlineData("fragment name onn Type { field }", "Unexpected token Name(onn)")] // Misspelled 'on'
        public void Parse_Throws_On_InvalidKeywordUsage(string source, string expectedMessagePart)
        {
            // This might actually throw at ParseOperationType or similar if not a valid operation type
            // For now, assuming ParseExecutableDocument is the entry point for testing these.
            AssertParsingThrows(source, expectedMessagePart);
        }

        [Theory]
        [InlineData("query { field (", "Unexpected token End")] // Unterminated arguments
        [InlineData("query { field { subfield ", "Unexpected token End")] // Unterminated selection set
        [InlineData("query ($v: Int = ", "Unexpected token End")] // Unterminated variable default value
        [InlineData("query ($v: [String ", "Unexpected token End")] // Unterminated list type
        [InlineData("directive @myDir(arg: ", "Unexpected token End")] // Unterminated directive arguments
        public void Parse_Throws_On_UnterminatedConstructs(string source, string expectedMessagePart)
        {
            AssertParsingThrows(source, expectedMessagePart);
        }

        [Theory]
        // These test if ParseValue(constant: true) is violated or if specific constructs disallow variables.
        // The parser's ParseValue has a 'constant' flag.
        // Default values for variables must be constant.
        [InlineData("query ($v: Int = $anotherVar) { field }", "Unexpected token Dollar")] 
        // Directive arguments in SDL (like field definition) must be constant.
        // This requires testing via ParseTypeSystemDocument or specific directive parsing contexts.
        // For executable documents, directives on fields/operations can take variables.
        // Example: field @skip(if: $myBooleanVariable) -> this is valid.
        // Let's test a case where a constant value is expected by a helper.
        // e.g. if ParseArgument calls ParseValue(constant: true) when it shouldn't.
        // This category is hard to test generically at ParseExecutableDocument if the structure itself is
        // not immediately invalid before value parsing context is known.
        // The example "$v: Int = $anotherVar" is a good one for variable definitions.
        public void Parse_Throws_On_InvalidVariableUsageInConstantContext(string source, string expectedMessagePart)
        {
            AssertParsingThrows(source, expectedMessagePart);
        }

        [Fact]
        public void Parse_Throws_On_FragmentNameIsOn()
        {
            // "on" is a keyword in fragment definitions, cannot be a fragment name.
            AssertParsingThrows("fragment on on Type { field }", "Unexpected token Name(on)");
        }
    }
    #endregion

    #region Parser Complex Structure Tests
    public class ParserComplexStructureFacts
    {
        [Fact]
        public void Parse_FullFeaturedQuery()
        {
            /* Given */
            var source = @"
                query MyQuery($var1: String = ""default"", $var2: [Int!]!) @skip(if: true) @customDir(arg: {
                    str: ""value"",
                    list: [1, ""two"", null, false, {objField: $var1}],
                    obj: {boolField: true, nullField: null}
                }) {
                    alias: field(arg1: $var1, arg2: 123, arg3: ENUM_VALUE) @include(if: false) {
                        subField1
                        ...frag1
                        ... on MyType @defer(label: ""myDeferLabel"", if: $var2) {
                            subField2
                        }
                        ... @include(if: true) {
                           inlineFieldNoType
                        }
                    }
                    anotherField: field # end of field comment
                }";

            var parser = Parser.Create(source);

            /* When */
            var document = parser.ParseExecutableDocument();

            /* Then */
            Assert.NotNull(document);
            Assert.Single(document.OperationDefinitions);

            var operation = document.OperationDefinitions.First();
            Assert.Equal("MyQuery", operation.Name?.Value);
            Assert.Equal(OperationType.Query, operation.Operation);
            Assert.True(operation.Location != null && operation.Location.Start > 0);

            // Variable Definitions
            Assert.NotNull(operation.VariableDefinitions);
            Assert.Equal(2, operation.VariableDefinitions.Count);
            var var1 = operation.VariableDefinitions.Single(v => v.Variable.Name.Value == "var1");
            Assert.Equal("String", ((NamedType)var1.Type).Name.Value);
            Assert.IsType<StringValue>(var1.DefaultValue?.Value);
            Assert.Equal("default", ((StringValue)var1.DefaultValue.Value).Value);

            var var2 = operation.VariableDefinitions.Single(v => v.Variable.Name.Value == "var2");
            var var2NonNullType = Assert.IsType<NonNullType>(var2.Type);
            var var2ListType = Assert.IsType<ListType>(var2NonNullType.OfType);
            var var2ListNonNullType = Assert.IsType<NonNullType>(var2ListType.OfType);
            Assert.Equal("Int", ((NamedType)var2ListNonNullType.OfType).Name.Value);

            // Directives on Operation
            Assert.NotNull(operation.Directives);
            Assert.Equal(2, operation.Directives.Count);
            var skipDirective = operation.Directives.Single(d => d.Name.Value == "skip");
            Assert.Single(skipDirective.Arguments);
            Assert.Equal("if", skipDirective.Arguments.First().Name.Value);
            Assert.IsType<BooleanValue>(skipDirective.Arguments.First().Value);

            var customDir = operation.Directives.Single(d => d.Name.Value == "customDir");
            Assert.Single(customDir.Arguments);
            var customDirArg = Assert.IsType<ObjectValue>(customDir.Arguments.First().Value);
            Assert.Equal(3, customDirArg.Fields.Count);
            Assert.Equal("value", ((StringValue)customDirArg.Fields.Single(f => f.Name.Value == "str").Value).Value);
            var listArg = Assert.IsType<ListValue>(customDirArg.Fields.Single(f => f.Name.Value == "list").Value);
            Assert.Equal(5, listArg.Values.Count);
            Assert.IsType<IntValue>(listArg.Values[0]);
            Assert.IsType<StringValue>(listArg.Values[1]);
            Assert.IsType<NullValue>(listArg.Values[2]);
            Assert.IsType<BooleanValue>(listArg.Values[3]);
            Assert.IsType<ObjectValue>(listArg.Values[4]);
            var objArg = Assert.IsType<ObjectValue>(customDirArg.Fields.Single(f => f.Name.Value == "obj").Value);
            Assert.Equal(2, objArg.Fields.Count);


            // SelectionSet
            Assert.NotNull(operation.SelectionSet);
            Assert.Equal(2, operation.SelectionSet.Count); 

            var mainFieldSelection = Assert.IsType<FieldSelection>(operation.SelectionSet.First(s => s is FieldSelection && ((FieldSelection)s).Name.Value == "field"));
            Assert.Equal("alias", mainFieldSelection.Alias?.Value);
            Assert.NotNull(mainFieldSelection.Arguments);
            Assert.Equal(3, mainFieldSelection.Arguments.Count);
            Assert.IsType<VariableValue>(mainFieldSelection.Arguments.Single(a => a.Name.Value == "arg1").Value);
            Assert.IsType<IntValue>(mainFieldSelection.Arguments.Single(a => a.Name.Value == "arg2").Value);
            Assert.IsType<EnumValue>(mainFieldSelection.Arguments.Single(a => a.Name.Value == "arg3").Value);

            Assert.NotNull(mainFieldSelection.Directives);
            Assert.Single(mainFieldSelection.Directives);
            Assert.Equal("include", mainFieldSelection.Directives.First().Name.Value);
            
            Assert.NotNull(mainFieldSelection.SelectionSet);
            Assert.Equal(4, mainFieldSelection.SelectionSet.Count);
            Assert.IsType<FieldSelection>(mainFieldSelection.SelectionSet[0]); // subField1
            Assert.IsType<FragmentSpread>(mainFieldSelection.SelectionSet[1]); // ...frag1
            var inlineFragment = Assert.IsType<InlineFragment>(mainFieldSelection.SelectionSet[2]); // ... on MyType
            Assert.Equal("MyType", inlineFragment.TypeCondition?.Name.Value);
            Assert.NotNull(inlineFragment.Directives);
            Assert.Single(inlineFragment.Directives);
            Assert.Equal("defer", inlineFragment.Directives.First().Name.Value);
            Assert.NotNull(inlineFragment.SelectionSet);
            Assert.Single(inlineFragment.SelectionSet);
            Assert.Equal("subField2", ((FieldSelection)inlineFragment.SelectionSet.First()).Name.Value);

            var inlineFragmentNoType = Assert.IsType<InlineFragment>(mainFieldSelection.SelectionSet[3]); // ... @include
            Assert.Null(inlineFragmentNoType.TypeCondition);
            Assert.Single(inlineFragmentNoType.Directives);
            Assert.Equal("inlineFieldNoType", ((FieldSelection)inlineFragmentNoType.SelectionSet.First()).Name.Value);

            var anotherField = Assert.IsType<FieldSelection>(operation.SelectionSet.First(s => s is FieldSelection && ((FieldSelection)s).Name.Value == "field" && ((FieldSelection)s).Alias?.Value == "anotherField"));
            Assert.Null(anotherField.SelectionSet); // No sub-selection for anotherField
        }

        [Fact]
        public void Parse_FullFeaturedMutation()
        {
            var source = @"
                mutation CreateReview($episode: Episode!, $review: ReviewInput!) @clientMutationId(id: ""123"") {
                    createReview(episode: $episode, review: $review) {
                        stars
                        commentary
                    }
                }";
            var parser = Parser.Create(source);
            var document = parser.ParseExecutableDocument();

            Assert.NotNull(document);
            Assert.Single(document.OperationDefinitions);
            var operation = document.OperationDefinitions.First();
            Assert.Equal(OperationType.Mutation, operation.Operation);
            Assert.Equal("CreateReview", operation.Name?.Value);
            Assert.Equal(2, operation.VariableDefinitions?.Count);
            Assert.Single(operation.Directives);
            Assert.Equal("clientMutationId", operation.Directives.First().Name.Value);

            var createReviewField = Assert.IsType<FieldSelection>(operation.SelectionSet.Single());
            Assert.Equal("createReview", createReviewField.Name.Value);
            Assert.Equal(2, createReviewField.Arguments?.Count);
            Assert.NotNull(createReviewField.SelectionSet);
            Assert.Equal(2, createReviewField.SelectionSet.Count);
            Assert.Equal("stars", ((FieldSelection)createReviewField.SelectionSet[0]).Name.Value);
            Assert.Equal("commentary", ((FieldSelection)createReviewField.SelectionSet[1]).Name.Value);
        }
        
        [Fact]
        public void Parse_FullFeaturedSubscription()
        {
            var source = @"
                subscription StoryLikeSubscription($input: StoryLikeSubscribeInput) @live {
                    storyLikeSubscribe(input: $input) {
                        story {
                            likeCount
                            viewerHasLiked
                        }
                        liker { id name }
                    }
                }";
            var parser = Parser.Create(source);
            var document = parser.ParseExecutableDocument();
            Assert.NotNull(document);
            Assert.Single(document.OperationDefinitions);
            var operation = document.OperationDefinitions.First();
            Assert.Equal(OperationType.Subscription, operation.Operation);
            Assert.Equal("StoryLikeSubscription", operation.Name?.Value);
            Assert.Single(operation.VariableDefinitions);
            Assert.Single(operation.Directives);
            Assert.Equal("live", operation.Directives.First().Name.Value);

            var storyLikeSubscribeField = Assert.IsType<FieldSelection>(operation.SelectionSet.Single());
            Assert.Equal("storyLikeSubscribe", storyLikeSubscribeField.Name.Value);
            Assert.Single(storyLikeSubscribeField.Arguments);
            Assert.NotNull(storyLikeSubscribeField.SelectionSet);
            Assert.Equal(2, storyLikeSubscribeField.SelectionSet.Count); // story and liker
            
            var storyField = Assert.IsType<FieldSelection>(storyLikeSubscribeField.SelectionSet.First(s => ((FieldSelection)s).Name.Value == "story"));
            Assert.NotNull(storyField.SelectionSet);
            Assert.Equal(2, storyField.SelectionSet.Count); // likeCount, viewerHasLiked

            var likerField = Assert.IsType<FieldSelection>(storyLikeSubscribeField.SelectionSet.First(s => ((FieldSelection)s).Name.Value == "liker"));
            Assert.NotNull(likerField.SelectionSet);
            Assert.Equal(2, likerField.SelectionSet.Count); // id, name
        }

        [Fact]
        public void Parse_DeeplyNestedStructure()
        {
            var source = @"
                query DeepNest {
                    l1: level1 {
                        l2: level2(arg: [ {n1: 1}, {n2: 2}]) {
                            l3: level3 {
                                l4: level4 {
                                    l5: level5 @directive {
                                        name
                                        value
                                    }
                                }
                            }
                        }
                    }
                }";
            var parser = Parser.Create(source);
            var document = parser.ParseExecutableDocument();
            Assert.NotNull(document);
            var op = document.OperationDefinitions.First();
            var l1 = Assert.IsType<FieldSelection>(op.SelectionSet.Single()); Assert.Equal("l1", l1.Alias.Value);
            var l2 = Assert.IsType<FieldSelection>(l1.SelectionSet.Single()); Assert.Equal("l2", l2.Alias.Value);
            Assert.Single(l2.Arguments);
            var l3 = Assert.IsType<FieldSelection>(l2.SelectionSet.Single()); Assert.Equal("l3", l3.Alias.Value);
            var l4 = Assert.IsType<FieldSelection>(l3.SelectionSet.Single()); Assert.Equal("l4", l4.Alias.Value);
            var l5 = Assert.IsType<FieldSelection>(l4.SelectionSet.Single()); Assert.Equal("l5", l5.Alias.Value);
            Assert.Single(l5.Directives);
            Assert.Equal(2, l5.SelectionSet.Count);
            Assert.Equal("name", ((FieldSelection)l5.SelectionSet[0]).Name.Value);
            Assert.True(((FieldSelection)l5.SelectionSet[0]).Location != null);
            Assert.Equal("value", ((FieldSelection)l5.SelectionSet[1]).Name.Value);
            Assert.True(((FieldSelection)l5.SelectionSet[1]).Location != null && ((FieldSelection)l5.SelectionSet[1]).Location.Start > l1.Location.Start);
        }
    }
    #endregion
}