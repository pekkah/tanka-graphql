using System.Threading.Tasks;
using NSubstitute;
using Tanka.GraphQL.Generator.Integration.Tests.Model;
using Tanka.GraphQL.ValueResolution;
using Xunit;

namespace Tanka.GraphQL.Generator.Integration.Tests
{
    public class NullabilityTestObjectController : NullabilityTestObjectControllerBase<NullabilityTestObject>
    {
    }

    public class GeneratedObjectControllerNullabilityFacts
    {
        public GeneratedObjectControllerNullabilityFacts()
        {
            _sut = new NullabilityTestObjectController();
        }

        private readonly NullabilityTestObjectController _sut;

        private IResolverContext CreateContext(object? objectValue)
        {
            var context = Substitute.For<IResolverContext>();
            context.ObjectValue.Returns(objectValue);
            return context;
        }

        [Fact]
        public async Task NonNull_Property_field_with_null_objectValue()
        {
            /* Given */
            var context = CreateContext(null);

            /* When */
            var result = await _sut.NonNullInt(context);

            /* Then */
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task NonNull_Property_field()
        {
            /* Given */
            var context = CreateContext(new NullabilityTestObject
            {
                NonNullInt = 1
            });

            /* When */
            var result = await _sut.NonNullInt(context);

            /* Then */
            Assert.Equal(1, result.Value);
        }

        [Fact]
        public async Task Nullable_Property_field_with_null_objectValue()
        {
            /* Given */
            var context = CreateContext(null);

            /* When */
            var result = await _sut.Int(context);

            /* Then */
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task Nullable_Property_field_with_null()
        {
            /* Given */
            var context = CreateContext(new NullabilityTestObject
            {
                Int = null
            });

            /* When */
            var result = await _sut.Int(context);

            /* Then */
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task Nullable_Property_field_with_value()
        {
            /* Given */
            var context = CreateContext(new NullabilityTestObject
            {
                Int = 1
            });

            /* When */
            var result = await _sut.Int(context);

            /* Then */
            Assert.Equal(1, result.Value);
        }
    }
}