using System.Diagnostics.Contracts;

namespace Tanka.GraphQL.Server.SourceGenerators.Tests;

public static class VerifyExtensions
{
    [Pure]
    public static SettingsTask VerifyTemplate(
        string? target)
    {
        return Verify(target, extension: "cs")
            .UseDirectory("Templates/Snapshots");
    }
}