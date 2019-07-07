using System;
using System.IO;
using System.Reflection;
using System.Text;
using tanka.graphql.schema;
using tanka.graphql.sdl;
using tanka.graphql.type;

namespace tanka.graphql.samples.chat.data.idl
{
    public static class IdlSchema
    {
        public static SchemaBuilder Load()
        {
            var idl = LoadIdlFromResource();
            return new SchemaBuilder()
                .Sdl(idl);
        }

        /// <summary>
        ///     Load schema from embedded resource
        /// </summary>
        /// <returns></returns>
        private static string LoadIdlFromResource()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceStream =
                assembly.GetManifestResourceStream("tanka.graphql.samples.chat.data.idl.schema.graphql");
            using (var reader =
                new StreamReader(resourceStream ?? throw new InvalidOperationException(), Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}