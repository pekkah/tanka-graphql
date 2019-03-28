using System;
using System.IO;
using System.Reflection;
using System.Text;
using tanka.graphql.introspection;
using tanka.graphql.type;
using tanka.graphql.type.converters;
using Xunit;

namespace tanka.graphql.tests.introspection
{
    public class DigitransitRemoteSchemaFacts
    {
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


        private static string GetDigitransitIntrospection()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceStream = assembly.GetManifestResourceStream("tanka.graphql.tests.digitransit.introspection");
            using (var reader =
                new StreamReader(resourceStream ?? throw new InvalidOperationException(), Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}