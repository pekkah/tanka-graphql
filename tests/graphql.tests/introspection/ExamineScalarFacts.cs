using tanka.graphql.introspection;
using tanka.graphql.type;
using Xunit;

namespace tanka.graphql.tests.introspection
{
    public class ExamineScalarFacts
    {
        private readonly Schema _schema;

        public ExamineScalarFacts()
        {
            _schema = new Schema(
                new ObjectType("Query",
                new Fields
                {
                    ["int"] = new Field(ScalarType.Int),
                    ["boolean"] = new Field(ScalarType.Boolean),
                    ["float"] = new Field(ScalarType.Float),
                    ["string"] = new Field(ScalarType.String),
                }));
        }

        [Fact(Skip = "fix me")]
        public void Examine()
        {
            /* Given */
            var scalars = ScalarType.Standard;

            /* When */
            /* Then */
            foreach (var scalarType in scalars)
            {
            }
        }

    }
}