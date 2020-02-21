using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Tanka.GraphQL.Generator.Integration.Tests.Model;
using Tanka.GraphQL.ValueResolution;
using Xunit;

namespace Tanka.GraphQL.Generator.Integration.Tests
{
    public class GeneratedSubscriptionControllerFacts
    {
        public abstract class SubscriptionController : SubscriptionControllerBase<Subscription>
        {
        }
            
        private readonly SubscriptionController _sut;

        public GeneratedSubscriptionControllerFacts()
        {
            _sut = Substitute.ForPartsOf<SubscriptionController>();
        }

        private IResolverContext CreateContext(
            object? objectValue
        )
        {
            var context = Substitute.For<IResolverContext>();
            context.ObjectValue.Returns(objectValue);

            return context;
        }

        [Fact]
        public async Task Subscribe_to_Int_no_args()
        {
            /* Given */
            var context = CreateContext(null);
 
            var expected = Substitute.For<ISubscriberResult>();
            _sut.Int(context, CancellationToken.None)
                .Returns(expected);

            /* When */
            var actual = await _sut.Int(context, CancellationToken.None);

            /* Then */
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task Subscribe_to_Int_one_Int_arg()
        {
            /* Given */
            var context = CreateContext(null);
            context.Arguments.Returns(new Dictionary<string, object>()
            {
                ["arg1"] = 1
            });

            var expected = Substitute.For<ISubscriberResult>();
            _sut.IntWithArgument(context, CancellationToken.None)
                .Returns(expected);

            /* When */
            var actual = await _sut.IntWithArgument(context, CancellationToken.None);

            /* Then */
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task Subscribe_to_Int_one_Int_and_one_String_arg()
        {
            /* Given */
            var context = CreateContext(null);
            context.Arguments.Returns(new Dictionary<string, object>()
            {
                ["arg1"] = 1,
                ["arg2"] = "string"
            });

            var expected = Substitute.For<ISubscriberResult>();
            _sut.IntWithTwoArguments(context, CancellationToken.None)
                .Returns(expected);

            /* When */
            var actual = await _sut.IntWithTwoArguments(context, CancellationToken.None);

            /* Then */
            Assert.Equal(expected, actual);
        }
    }
}