using System;
using System.Runtime.CompilerServices;
using System.Text;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Language;

public static class Extensions
{
    public static bool Match(this in ReadOnlyMemory<byte> memory, in ReadOnlySpan<byte> value)
    {
        return memory.Span.SequenceEqual(value);
    }
}

public static class Keywords
{
    public static ReadOnlyMemory<byte> Query = new(Encoding.UTF8.GetBytes("query"));

    public static ReadOnlyMemory<byte> Mutation = new(Encoding.UTF8.GetBytes("mutation"));

    public static ReadOnlyMemory<byte> Subscription = new(Encoding.UTF8.GetBytes("subscription"));

    public static ReadOnlyMemory<byte> Fragment = new(Encoding.UTF8.GetBytes("fragment"));

    public static ReadOnlyMemory<byte> Null = new(Encoding.UTF8.GetBytes("null"));

    public static ReadOnlyMemory<byte> True = new(Encoding.UTF8.GetBytes("true"));

    public static ReadOnlyMemory<byte> False = new(Encoding.UTF8.GetBytes("false"));

    public static ReadOnlyMemory<byte> True2 = new(Encoding.UTF8.GetBytes("True"));

    public static ReadOnlyMemory<byte> False2 = new(Encoding.UTF8.GetBytes("False"));

    public static ReadOnlyMemory<byte> On = new(Encoding.UTF8.GetBytes("on"));

    public static ReadOnlyMemory<byte> Repeatable = new(Encoding.UTF8.GetBytes("repeatable"));

    public static ReadOnlyMemory<byte> Schema = new(
        Encoding.UTF8.GetBytes("schema"));

    public static ReadOnlyMemory<byte> Directive = new(
        Encoding.UTF8.GetBytes("directive"));

    public static ReadOnlyMemory<byte> Type = new(
        Encoding.UTF8.GetBytes("type"));

    public static ReadOnlyMemory<byte> Scalar = new(
        Encoding.UTF8.GetBytes("scalar"));

    public static ReadOnlyMemory<byte> Interface = new(
        Encoding.UTF8.GetBytes("interface"));

    public static ReadOnlyMemory<byte> Extend = new(
        Encoding.UTF8.GetBytes("extend"));

    public static ReadOnlyMemory<byte> Implements = new(
        Encoding.UTF8.GetBytes("implements"));

    public static ReadOnlyMemory<byte> Union = new(
        Encoding.UTF8.GetBytes("union"));

    public static ReadOnlyMemory<byte> Enum = new(
        Encoding.UTF8.GetBytes("enum"));

    public static ReadOnlyMemory<byte> Input = new(
        Encoding.UTF8.GetBytes("input"));

    public static ReadOnlyMemory<byte> Import = new(
        Encoding.UTF8.GetBytes("tanka_import"));

    public static ReadOnlyMemory<byte> From = new(
        Encoding.UTF8.GetBytes("from"));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBoolean(in ReadOnlySpan<byte> span, out bool value)
    {
        if (True.Match(span) || True2.Match(span))
        {
            value = true;
            return true;
        }

        if (False.Match(span) || False2.Match(span))
        {
            value = false;
            return true;
        }

        value = false;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsFragment(in ReadOnlySpan<byte> value)
    {
        return Fragment.Span.SequenceEqual(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsImplements(in ReadOnlySpan<byte> value)
    {
        return Implements.Span.SequenceEqual(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNull(in ReadOnlySpan<byte> span)
    {
        return Null.Span.SequenceEqual(span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOn(in ReadOnlySpan<byte> value)
    {
        return On.Span.SequenceEqual(value);
    }

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

        operation = default(OperationType);
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRepeatable(in ReadOnlySpan<byte> value)
    {
        return Repeatable.Span.SequenceEqual(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsTypeDefinition(in ReadOnlySpan<byte> value)
    {
        if (Scalar.Match(value))
            return true;

        if (Type.Match(value))
            return true;

        if (Interface.Match(value))
            return true;

        if (Union.Match(value))
            return true;

        if (Enum.Match(value))
            return true;

        if (Input.Match(value))
            return true;

        return false;
    }
}