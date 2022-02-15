using System;
using System.Runtime.CompilerServices;

namespace Tanka.GraphQL.Language.Internal;

internal ref struct SpanReader
{
    public readonly ReadOnlySpan<byte> Span;

    public int Position;

    public long Length => Span.Length;

    public SpanReader(in ReadOnlySpan<byte> span)
    {
        Span = span;
        Position = -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryRead(out byte value)
    {
        Position++;
        if (Position >= Length)
        {
            Position--;
            value = default;
            return false;
        }

        value = Span[Position];
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPeek(out byte value)
    {
        var nextPosition = Position + 1;
        if (nextPosition >= Length)
        {
            value = default;
            return false;
        }

        value = Span[nextPosition];
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Advance(in int count = 1)
    {
        var newPosition = Position += count;

        if (newPosition >= Length)
            return false;

        Position = newPosition;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsNext(in ReadOnlySpan<byte> maybeNext)
    {
        var start = Position + 1;
        if (maybeNext.Length + start > Length)
            return false;


        var next = Span.Slice(start, maybeNext.Length);
        return next.SequenceEqual(maybeNext);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TrySkipNext(in ReadOnlySpan<byte> maybeNext)
    {
        if (IsNext(maybeNext))
        {
            Position += maybeNext.Length;
            return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TrySkipNext(in byte maybeNext)
    {
        if (TryPeek(out var value))
            if (maybeNext == value)
            {
                Advance();
                return true;
            }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryReadWhileAny(
        out ReadOnlySpan<byte> data,
        in bool[] matchTable)
    {
        var start = Position + 1;
        var length = 0;
        while (TryPeek(out var value))
        {
            if (!matchTable[value]) break;

            Advance();
            length++;
        }

        if (length == 0)
        {
            data = ReadOnlySpan<byte>.Empty;
            return false;
        }

        data = Span.Slice(start, length);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryReadWhileNotAny(
        out ReadOnlySpan<byte> data,
        in bool[] matchTable)
    {
        var start = Position + 1;
        var length = 0;
        while (TryPeek(out var value))
        {
            if (matchTable[value]) break;

            Advance();
            length++;
        }

        if (length == 0)
        {
            data = ReadOnlySpan<byte>.Empty;
            return false;
        }

        data = Span.Slice(start, length);
        return true;
    }

    public bool TryRead(out ReadOnlySpan<byte> value, int count)
    {
        if (Position + count >= Length)
        {
            value = default;
            return false;
        }

        Position++;
        value = Span.Slice(Position, count);
        return true;
    }
}