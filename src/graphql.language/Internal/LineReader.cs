using System;

namespace Tanka.GraphQL.Language.Internal
{
    internal ref struct LineReader
    {
        private readonly ReadOnlySpan<byte> _span;

        private int Position { get; set; }

        public LineReader(in ReadOnlySpan<byte> span)
        {
            _span = span;
            Position = -1;
        }

        public bool TryReadLine(out ReadOnlySpan<byte> line)
        {
            Position++;
            if (Position >= _span.Length)
            {
                line = default;
                Position = _span.Length - 1;
                return false;
            }

            var unreadSpan = _span.Slice(Position);
            var newLineOrReturnIndex = unreadSpan
                .IndexOfAny(Constants.Return, Constants.NewLine);

            // no line found
            if (newLineOrReturnIndex == -1)
            {
                line = unreadSpan;
                Position = _span.Length - 1;
                return true;
            }

            Position += newLineOrReturnIndex;
            // skip \r
            if (unreadSpan[newLineOrReturnIndex] == Constants.Return) Position++;

            line = unreadSpan.Slice(0, newLineOrReturnIndex);
            return true;
        }
    }
}