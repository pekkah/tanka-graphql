using System;
using System.Runtime.CompilerServices;

using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Language;

public static class Extensions
{
    public static bool Match(this in ReadOnlySpan<byte> memory, in ReadOnlySpan<byte> value)
    {
        return memory.SequenceEqual(value);
    }
}

public static class Keywords
{
    public static ReadOnlySpan<byte> Query => "query"u8;

    public static ReadOnlySpan<byte> Mutation => "mutation"u8;

    public static ReadOnlySpan<byte> Subscription => "subscription"u8;

    public static ReadOnlySpan<byte> Fragment => "fragment"u8;

    public static ReadOnlySpan<byte> Null => "null"u8;

    public static ReadOnlySpan<byte> True => "true"u8;

    public static ReadOnlySpan<byte> False => "false"u8;

    public static ReadOnlySpan<byte> True2 => "True"u8;

    public static ReadOnlySpan<byte> False2 => "False"u8;

    public static ReadOnlySpan<byte> On => "on"u8;

    public static ReadOnlySpan<byte> Repeatable => "repeatable"u8;

    public static ReadOnlySpan<byte> Schema => "schema"u8;

    public static ReadOnlySpan<byte> Directive => "directive"u8;

    public static ReadOnlySpan<byte> Type => "type"u8;

    public static ReadOnlySpan<byte> Scalar => "scalar"u8;

    public static ReadOnlySpan<byte> Interface => "interface"u8;

    public static ReadOnlySpan<byte> Extend => "extend"u8;

    public static ReadOnlySpan<byte> Implements => "implements"u8;

    public static ReadOnlySpan<byte> Union => "union"u8;

    public static ReadOnlySpan<byte> Enum => "enum"u8;

    public static ReadOnlySpan<byte> Input => "input"u8;


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
        return Fragment.SequenceEqual(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsImplements(in ReadOnlySpan<byte> value)
    {
        return Implements.SequenceEqual(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNull(in ReadOnlySpan<byte> span)
    {
        return Null.SequenceEqual(span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOn(in ReadOnlySpan<byte> value)
    {
        return On.SequenceEqual(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOperation(in ReadOnlySpan<byte> span, out OperationType operation)
    {
        if (Query.SequenceEqual(span))
        {
            operation = OperationType.Query;
            return true;
        }

        if (Mutation.SequenceEqual(span))
        {
            operation = OperationType.Mutation;
            return true;
        }

        if (Subscription.SequenceEqual(span))
        {
            operation = OperationType.Subscription;
            return true;
        }

        operation = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRepeatable(in ReadOnlySpan<byte> value)
    {
        return Repeatable.SequenceEqual(value);
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