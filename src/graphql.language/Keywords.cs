using System;
using System.Runtime.CompilerServices;
using System.Text;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Language
{
    public static class Keywords
    {
        public static ReadOnlyMemory<byte> Query
            = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes("query"));

        public static ReadOnlyMemory<byte> Mutation
            = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes("mutation"));

        public static ReadOnlyMemory<byte> Subscription
            = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes("subscription"));

        public static ReadOnlyMemory<byte> Fragment
            = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes("fragment"));

        public static ReadOnlyMemory<byte> Null
            = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes("null"));

        public static ReadOnlyMemory<byte> True
            = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes("true"));

        public static ReadOnlyMemory<byte> False
            = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes("false"));

        public static ReadOnlyMemory<byte> On
            = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes("on"));

        public static ReadOnlyMemory<byte> Repeatable
            = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes("repeatable"));

        public static ReadOnlyMemory<byte> Schema = new ReadOnlyMemory<byte>(
            Encoding.UTF8.GetBytes("schema"));

        public static ReadOnlyMemory<byte> Directive = new ReadOnlyMemory<byte>(
            Encoding.UTF8.GetBytes("directive"));

        public static ReadOnlyMemory<byte> Type = new ReadOnlyMemory<byte>(
            Encoding.UTF8.GetBytes("type"));

        public static ReadOnlyMemory<byte> Scalar = new ReadOnlyMemory<byte>(
            Encoding.UTF8.GetBytes("scalar"));

        public static ReadOnlyMemory<byte> Interface = new ReadOnlyMemory<byte>(
            Encoding.UTF8.GetBytes("interface"));

        public static ReadOnlyMemory<byte> Extend = new ReadOnlyMemory<byte>(
            Encoding.UTF8.GetBytes("extend"));

        public static ReadOnlyMemory<byte> Implements = new ReadOnlyMemory<byte>(
            Encoding.UTF8.GetBytes("implements"));

        public static ReadOnlyMemory<byte> Union = new ReadOnlyMemory<byte>(
            Encoding.UTF8.GetBytes("union"));

        public static ReadOnlyMemory<byte> Enum = new ReadOnlyMemory<byte>(
            Encoding.UTF8.GetBytes("enum"));

        public static ReadOnlyMemory<byte> Input = new ReadOnlyMemory<byte>(
            Encoding.UTF8.GetBytes("input"));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOperation(in ReadOnlySpan<byte> span, out OperationType operation)
        {
            if (Query.Span.SequenceEqual(span))
            {
                operation = OperationType.Query;
                return true;
            }

            if (Mutation.Span.SequenceEqual(span))
            {
                operation = OperationType.Mutation;
                return true;
            }

            if (Subscription.Span.SequenceEqual(span))
            {
                operation = OperationType.Subscription;
                return true;
            }

            operation = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNull(in ReadOnlySpan<byte> span)
        {
            return Null.Span.SequenceEqual(span);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOn(in ReadOnlySpan<byte> value)
        {
            return On.Span.SequenceEqual(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFragment(in ReadOnlySpan<byte> value)
        {
            return Fragment.Span.SequenceEqual(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsRepeatable(in ReadOnlySpan<byte> value)
        {
            return Repeatable.Span.SequenceEqual(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsImplements(in ReadOnlySpan<byte> value)
        {
            return Implements.Span.SequenceEqual(value);
        }
    }
}