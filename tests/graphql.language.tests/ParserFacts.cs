using Tanka.GraphQL.Language.Nodes;
using Xunit;

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
        public void OperationDefinition_With_Comment_Betweeen_Selections()
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
    }
}