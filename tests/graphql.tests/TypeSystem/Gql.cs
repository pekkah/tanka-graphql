using System.Text.RegularExpressions;
using Xunit;

public static class Gql
{
    public static void AssertEqual(string expected, string actual)
    {
        string Normalize(string str)
        {
            str = str
                .Replace("\r", string.Empty)
                .Replace("\n", " ")
                .Trim();

            return Regex.Replace(str, @"\s+", " ");
        }

        Assert.Equal(
            Normalize(expected),
            Normalize(actual),
            false,
            true,
            true
        );
    }
}