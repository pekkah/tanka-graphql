using System;
using System.IO;
using System.Reflection;
using System.Text;
using Xunit;

namespace Tanka.GraphQL.Generator.Integration.Tests
{
    public class GraphQLEmbeddedAsResourceFacts
    {
        [Fact]
        public void EmbeddedAsResource()
        {
            Assert.NotNull(LoadIdlFromResource());
        }

        private static string LoadIdlFromResource()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceStream =
                assembly.GetManifestResourceStream("Tanka.GraphQL.Generator.Integration.Tests.Model.Schema.graphql")
                ?? throw new InvalidOperationException("Could not load resource");

            using var reader =
                new StreamReader(resourceStream , Encoding.UTF8);

            return reader.ReadToEnd();
        }
    }
}