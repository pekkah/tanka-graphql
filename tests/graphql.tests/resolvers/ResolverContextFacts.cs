using System.Collections.Generic;
using fugu.graphql.execution;
using fugu.graphql.resolvers;
using fugu.graphql.type;
using GraphQLParser.AST;
using Xunit;

namespace fugu.graphql.tests.resolvers
{
    public class ResolverContextFacts
    {
        public ResolverContextFacts()
        {
            _objectType = new ObjectType("Test", new Fields());
            _objectValue = null;
            _field = new Field(ScalarType.ID);
            _selection = new GraphQLFieldSelection();
        }

        private readonly IField _field;
        private readonly ObjectType _objectType;
        private readonly object _objectValue;
        private readonly GraphQLFieldSelection _selection;

        private class InputArg
        {
            public string Name { get; set; }
        }

        [Fact]
        public void Get_double_argument()
        {
            /* Given */
            var arguments = new Dictionary<string, object>
            {
                {"double", 100.1D}
            };

            var sut = new ResolverContext(_objectType, _objectValue, _field, _selection, arguments, new NodePath());

            /* When */
            var value = sut.GetArgument<double>("double");

            /* Then */
            Assert.Equal(100.1D, value);
        }

        [Fact]
        public void Get_float_argument()
        {
            /* Given */
            var arguments = new Dictionary<string, object>
            {
                {"float", 100.1F}
            };

            var sut = new ResolverContext(_objectType, _objectValue, _field, _selection, arguments, new NodePath());

            /* When */
            var value = sut.GetArgument<float>("float");

            /* Then */
            Assert.Equal(100.1F, value);
        }

        [Fact]
        public void Get_int_argument()
        {
            /* Given */
            var arguments = new Dictionary<string, object>
            {
                {"int", 101}
            };

            var sut = new ResolverContext(_objectType, _objectValue, _field, _selection, arguments, new NodePath());

            /* When */
            var value = sut.GetArgument<int>("int");

            /* Then */
            Assert.Equal(101, value);
        }

        [Fact]
        public void Get_long_argument()
        {
            /* Given */
            var arguments = new Dictionary<string, object>
            {
                {"long", 100L}
            };

            var sut = new ResolverContext(_objectType, _objectValue, _field, _selection, arguments, new NodePath());

            /* When */
            var value = sut.GetArgument<long>("long");

            /* Then */
            Assert.Equal(100L, value);
        }

        [Fact]
        public void Get_object_argument()
        {
            /* Given */
            var arguments = new Dictionary<string, object>
            {
                {
                    "input", new Dictionary<string, object>
                    {
                        {"name", "inputArg"}
                    }
                }
            };

            var sut = new ResolverContext(_objectType, _objectValue, _field, _selection, arguments, new NodePath());

            /* When */
            var value = sut.GetArgument<InputArg>("input");

            /* Then */
            Assert.Equal("inputArg", value.Name);
        }

        [Fact]
        public void Get_string_argument()
        {
            /* Given */
            var arguments = new Dictionary<string, object>
            {
                {"string", "101"}
            };

            var sut = new ResolverContext(_objectType, _objectValue, _field, _selection, arguments, new NodePath());

            /* When */
            var value = sut.GetArgument<string>("string");

            /* Then */
            Assert.Equal("101", value);
        }
    }
}