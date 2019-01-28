using System.Collections.Generic;
using System.Linq;
using tanka.graphql.execution;
using tanka.graphql.type;
using Xunit;

namespace tanka.graphql.tests.execution
{
    public class TypeIsFacts
    {
        [Theory]
        [MemberData(nameof(ValidInputTypes))]
        public void IsInputType(INamedType type)
        {
            /* Given */
            /* When */
            var isInput = TypeIs.IsInputType(type);
            var isInputAsNonNull = TypeIs.IsInputType(
                new NonNull(type));
            var isInputAsList = TypeIs.IsInputType(
                new List(type));

            /* Then */
            Assert.True(isInput);
            Assert.True(isInputAsNonNull);
            Assert.True(isInputAsList);
        }

        [Theory]      
        [MemberData(nameof(ValidOutputTypes))]
        public void IsOutputType(INamedType type)
        {
            /* Given */
            /* When */
            var isOutput = TypeIs.IsOutputType(type);
            var isOutputAsNonNull = TypeIs.IsOutputType(
                new NonNull(type));
            var isOutputAsList = TypeIs.IsOutputType(
                new List(type));

            /* Then */
            Assert.True(isOutput);
            Assert.True(isOutputAsNonNull);
            Assert.True(isOutputAsList);
        }

        public static IEnumerable<object[]> ValidInputTypes
        {
            get
            {
                foreach (var scalarType in ScalarType.Standard)
                {
                    yield return new object[] {scalarType};
                }

                yield return new object[]
                {
                    new EnumType("Enum",
                        new EnumValues())
                };

                yield return new object[]
                {
                    new InputObjectType("Input",
                        new InputFields())
                };
            }
        }

        public static IEnumerable<object[]> ValidOutputTypes
        {
            get
            {
                foreach (var scalarType in ScalarType.Standard)
                {
                    yield return new object[] {scalarType};
                }

                yield return new object[]
                {
                    new ObjectType("Object",
                        new Fields()), 
                };

                yield return new object[]
                {
                    new InterfaceType("Interface",
                        new Fields()), 
                };

                yield return new object[]
                {
                    new UnionType("Union",
                        Enumerable.Empty<INamedType>()), 
                };

                yield return new object[]
                {
                    new EnumType("Enum",
                        new EnumValues())
                };
            }
        }
    }
}