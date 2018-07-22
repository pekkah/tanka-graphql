using System.Threading.Tasks;
using fugu.graphql.tests.data.validation;
using Xunit;
using static fugu.graphql.Parser;
using static fugu.graphql.validation.Validator;

namespace fugu.graphql.tests.validation
{
    public class ValidatorFacts
    {
        public ValidatorFacts()
        {
            _schema = new WeirdSchema();
            _schema.InitializeAsync().Wait();
        }

        private readonly WeirdSchema _schema;

        [Fact]
        public async Task Document_IsExecutable()
        {
            /* Given */
            var query = @"
query getDogName {
  dog {
    name
    color
  }
}

extend type Dog {
  color: String
}";

            /* When */
            var result = await ValidateAsync(
                _schema,
                ParseDocument(query));

            /* Then */
            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task Fields_Selections()
        {
            /* Given */
            var query = @"
fragment fieldNotDefined on Dog {
  meowVolume
}
";

            /* When */
            var result = await ValidateAsync(
                _schema,
                ParseDocument(query));

            /* Then */
            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task Operations_LoneAnonymousOperation()
        {
            /* Given */
            var query = @"
{
  dog {
    name
  }
}

query getName {
  dog {
    owner {
      name
    }
  }
}";

            /* When */
            var result = await ValidateAsync(
                _schema,
                ParseDocument(query));

            /* Then */
            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task Operations_UniqueNames()
        {
            /* Given */
            var query = @"
query getName {
  dog {
    name
  }
}

query getName {
  dog {
    owner {
      name
    }
  }
}";

            /* When */
            var result = await ValidateAsync(
                _schema,
                ParseDocument(query));

            /* Then */
            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task Operations_UniqueNames_even_when_different_types()
        {
            /* Given */
            var query = @"
query dogOperation {
  dog {
    name
  }
}

mutation dogOperation {
  mutateDog {
    id
  }
}";

            /* When */
            var result = await ValidateAsync(
                _schema,
                ParseDocument(query));

            /* Then */
            Assert.False(result.IsValid);
        }
    }
}