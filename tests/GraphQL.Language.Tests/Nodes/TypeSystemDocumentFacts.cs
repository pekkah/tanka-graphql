using System.Text;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Language.Tests.Nodes;

public class TypeSystemDocumentFacts
{
    [Fact]
    public void FromBytes()
    {
        /* Given */
        /* When */
        TypeSystemDocument original = Encoding.UTF8.GetBytes("scalar Scalar extend scalar Scalar @a")
            .AsReadOnlySpan();

        /* Then */
        Assert.NotNull(original.TypeDefinitions);
        Assert.Single(original.TypeDefinitions);

        Assert.NotNull(original.TypeExtensions);
        Assert.Single(original.TypeExtensions);
    }

    [Fact]
    public void FromString()
    {
        /* Given */
        /* When */
        TypeSystemDocument original = "scalar Scalar extend scalar Scalar @a";

        /* Then */
        Assert.NotNull(original.TypeDefinitions);
        Assert.Single(original.TypeDefinitions);

        Assert.NotNull(original.TypeExtensions);
        Assert.Single(original.TypeExtensions);
    }
}