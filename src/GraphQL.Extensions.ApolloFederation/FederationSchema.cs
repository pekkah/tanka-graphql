using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.SchemaBuilding;

namespace Tanka.GraphQL.Extensions.ApolloFederation
{
    public class FederationSchema
    {
        public static TypeSystemDocument TypesDocument =>
            Parser.ParseTypeSystemDocument(GetTypesSdl());

        private static string GetTypesSdl()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceStream =
                assembly.GetManifestResourceStream(
                    "Tanka.GraphQL.Extensions.ApolloFederation.Types.graphql");

            using var reader =
                new StreamReader(resourceStream ?? throw new InvalidOperationException(
                    "Could not load Types.graphql resource"), Encoding.UTF8);

            return reader.ReadToEnd();
        }
    }
}
