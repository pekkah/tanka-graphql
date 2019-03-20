using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using tanka.graphql.sdl;
using tanka.graphql.type;
using static tanka.graphql.Parser;

namespace tanka.graphql.samples.chat.data.idl
{
    public static class IdlSchema
    {
        public static async Task<ISchema> CreateAsync()
        {
            var idl = await LoadIdlFromResourcesAsync();
            var schema = Sdl.Schema(ParseDocument(idl));

            // this will initialize schema by scanning the type graph
            ;
            return schema;
        }

        /// <summary>
        ///     Load schema from embedded resource
        /// </summary>
        /// <returns></returns>
        private static async Task<string> LoadIdlFromResourcesAsync()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceStream = assembly.GetManifestResourceStream("tanka.graphql.samples.chat.data.idl.schema.graphql");
            using (var reader =
                new StreamReader(resourceStream ?? throw new InvalidOperationException(), Encoding.UTF8))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}