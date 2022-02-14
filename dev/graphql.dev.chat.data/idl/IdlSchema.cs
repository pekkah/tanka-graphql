using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace Tanka.GraphQL.Samples.Chat.Data.IDL;

public static class IdlSchema
{
    public static SchemaBuilder Load()
    {
        var idl = LoadIdlFromResource();
        return new SchemaBuilder()
            .Add(idl);
    }

    /// <summary>
    ///     Load schema from embedded resource
    /// </summary>
    /// <returns></returns>
    private static string LoadIdlFromResource()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceStream =
            assembly.GetManifestResourceStream("Tanka.GraphQL.Samples.Chat.Data.IDL.schema.graphql");

        using var reader =
            new StreamReader(resourceStream ?? throw new InvalidOperationException(), Encoding.UTF8);

        return reader.ReadToEnd();
    }
}