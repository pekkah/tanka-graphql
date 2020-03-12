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
        public void OperationDefinition_SelectionSet_Selection()
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
        public void Field()
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
        public void Field_with_Alias()
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
        public void Field_SelectionSet()
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
            var source = "";

            var sut = Parser.Create(source);

            /* When */
            var actual = sut.ParseVariableDefinitions();

            /* Then */
            
        }

        [Fact]
        public void VariableDefinition()
        {
            /* Given */
            var source = "";

            var sut = Parser.Create(source);

            /* When */
            var actual = sut.ParseVariableDefinition();

            /* Then */
            
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
            Assert.IsType<NonNullOf>(type);
            Assert.IsType<NamedType>(((NonNullOf)type).OfType);
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
            Assert.IsType<ListOf>(type);
            Assert.IsType<NamedType>(((ListOf)type).OfType);
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
            Assert.IsType<NonNullOf>(type);
            Assert.IsType<ListOf>(((NonNullOf)type).OfType);
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
            Assert.IsType<ListOf>(type);
            Assert.IsType<NonNullOf>(((ListOf)type).OfType);
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
            var nonNullOf = Assert.IsType<NonNullOf>(type);
            var listOf = Assert.IsType<ListOf>(nonNullOf.OfType);
            var nonNullItemOf = Assert.IsType<NonNullOf>(listOf.OfType);
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
            Assert.Equal(expected, floatValue.Value);
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
            Assert.Equal(expected, stringValue.Value);
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
            Assert.Equal(expected, blockStringValue.Value);
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
            Assert.Equal(expected, enumValue.Value);
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
            Assert.Equal(0, listValue.Value.Count);
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
            Assert.Equal(3, listValue.Value.Count);
            Assert.All(listValue.Value, v => Assert.IsType<IntValue>(v));
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