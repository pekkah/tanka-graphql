using System.Threading.Tasks;
using tanka.graphql.type;
using tanka.graphql.validation;
using Xunit;

namespace tanka.graphql.tests.validation
{
    public class SubscriptionValidationFacts
    {
        public SubscriptionValidationFacts()
        {
            var message = new ObjectType(
                "Message",
                new Fields
                {
                    ["body"] = new Field(ScalarType.String),
                    ["sender"] = new Field(ScalarType.String)
                });

            var subscription = new ObjectType(
                "Subscription",
                new Fields
                {
                    ["newMessage"] = new Field(message),
                    ["disallowedSecondRootField"] = new Field(ScalarType.String)
                });

            var query = new ObjectType(
                "Query",
                new Fields());
            _schema = new Schema(query, null, subscription);
        }

        private readonly Schema _schema;

        [Fact]
        public async Task Subscriptions_SingleRootField()
        {
            /* Given */
            var query = @"
subscription sub {
  newMessage {
    body
    sender
  }
  disallowedSecondRootField
}";

            /* When */
            var result = await Validator.ValidateAsync(
                _schema,
                Parser.ParseDocument(query));

            /* Then */
            Assert.False(result.IsValid);
        }
    }
}