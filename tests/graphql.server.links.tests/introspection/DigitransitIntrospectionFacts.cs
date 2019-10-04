using System;
using System.IO;
using System.Reflection;
using System.Text;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.TypeSystem.ValueSerialization;
using Xunit;

namespace Tanka.GraphQL.Server.Links.Tests.Introspection
{
    public class DigitransitIntrospectionFacts
    {
        private static string GetDigitransitIntrospection()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceStream = assembly.GetManifestResourceStream("Tanka.GraphQL.Server.Links.Tests.digitransit.introspection");
            using (var reader =
                new StreamReader(resourceStream ?? throw new InvalidOperationException(), Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        [Fact]
        public void Read_types()
        {
            /* Given */
            var builder = new SchemaBuilder();
            builder.Scalar("Long", out _, new StringConverter());
            builder.Scalar("Lat", out _, new StringConverter());
            builder.Scalar("Polyline", out _, new StringConverter());

            /* When */
            builder.ImportIntrospectedSchema(GetDigitransitIntrospection());

            /* Then */
            Assert.True(builder.TryGetType<ObjectType>("Query", out var query));
        }
    }
}