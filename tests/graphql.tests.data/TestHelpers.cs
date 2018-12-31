using System;
using System.Globalization;
using System.IO;
using System.Text;
using Xunit;

namespace tanka.graphql.tests.data
{
    public static class TestHelpers
    {
        public static string Diff(string actualValue, string expectedValue)
        {
            return Diff(actualValue, expectedValue, DiffStyle.Minimal, new StringBuilder());
        }

        public static string Diff(string actualValue, string expectedValue, DiffStyle diffStyle, StringBuilder output)
        {
            if(actualValue == null || expectedValue == null)
            {
                return string.Empty;
            }

            if (actualValue.Equals(expectedValue, StringComparison.Ordinal)) 
                return string.Empty;

            output.AppendLine("  Idx Expected  Actual");
            output.AppendLine("-------------------------");
            int maxLen = Math.Max(actualValue.Length, expectedValue.Length);
            int minLen = Math.Min(actualValue.Length, expectedValue.Length);
            for (int i = 0; i < maxLen; i++)
            {
                if (diffStyle != DiffStyle.Minimal || i >= minLen || actualValue[i] != expectedValue[i])
                {
                    output.AppendLine($"{(i < minLen && actualValue[i] == expectedValue[i] ? " " : "*")} {i,-3} {(i < expectedValue.Length ? ((int)expectedValue[i]).ToString() : ""),-4} {(i < expectedValue.Length ? expectedValue[i].ToSafeString() : ""),-3}  {(i < actualValue.Length ? ((int)actualValue[i]).ToString() : ""),-4} {(i < actualValue.Length ? actualValue[i].ToSafeString() : ""),-3}" // character safe string
                    );
                }
            }
            output.AppendLine();

            return output.ToString();
        }

        private static string ToSafeString(this char c)
        {
            if (Char.IsControl(c) || Char.IsWhiteSpace(c))
            {
                switch (c)
                {
                    case '\r':
                        return @"\r";
                    case '\n':
                        return @"\n";
                    case '\t':
                        return @"\t";
                    case '\a':
                        return @"\a";
                    case '\v':
                        return @"\v";
                    case '\f':
                        return @"\f";
                    default:
                        return String.Format("\\u{0:X};", (int)c);
                }
            }
            return c.ToString(CultureInfo.InvariantCulture);
        }
    }
}