using System;

namespace Tanka.GraphQL.Language.Tests
{
    public static class SpanExtensions 
    {
        public static ReadOnlySpan<byte> AsReadOnlySpan(this byte[] bytes)
        {
            return bytes.AsSpan();
        }
    }
}