using System.Collections.Generic;
using System.Linq;
using Tanka.GraphQL.Execution;
using Tanka.GraphQL.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Tests.Execution
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
                    new InputObjectType("Input")
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
                    new ObjectType("Object"), 
                };

                yield return new object[]
                {
                    new InterfaceType("Interface"), 
                };

                yield return new object[]
                {
                    new UnionType("Union",
                        Enumerable.Empty<ObjectType>()), 
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