using System;
using System.Net.Http.Headers;
using System.Text;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Language
{
    public static class Keywords
    {
        public static ReadOnlyMemory<byte> Query
            = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes("query"));

        public static ReadOnlyMemory<byte> Null
            = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes("null"));

        public static ReadOnlyMemory<byte> True
            = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes("true"));

        public static ReadOnlyMemory<byte> False
            = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes("false"));

        public static bool IsOperation(ReadOnlySpan<byte> span, out OperationType operation)
        {
            if (Query.Span.SequenceEqual(span))
            {
                operation = OperationType.Query;
                return true;
            }

            operation = default;
            return false;
        }

        public static bool IsNull(in ReadOnlySpan<byte> span)
        {
            return Null.Span.SequenceEqual(span);
        }

        public static bool IsBoolean(in ReadOnlySpan<byte> span, out bool value)
        {
            if (True.Span.SequenceEqual(span))
            {
                value = true;
                return true;
            }

            if (False.Span.SequenceEqual(span))
            {
                value = false;
                return true;
            }

            value = false;
            return false;
        }
    }
}