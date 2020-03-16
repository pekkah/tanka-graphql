using System;
using System.Buffers;

namespace Tanka.GraphQL.Language.Internal
{
    internal readonly ref struct BlockStringValueReader
    {
        private readonly ReadOnlySpan<byte> _rawValue;

        public BlockStringValueReader(in ReadOnlySpan<byte> value)
        {
            _rawValue = value;
        }

        public ReadOnlySpan<byte> Read()
        {
            var commonIndent = CommonIndent(_rawValue);
            var trimWriter = new ArrayBufferWriter<byte>(_rawValue.Length);
            var lineReader = new LineReader(_rawValue);

            if (!lineReader.TryReadLine(out var firstLine)) throw new Exception("Could not read line starting");

            // trim
            trimWriter.Write(firstLine);
            if (commonIndent != null && commonIndent > 0)
                while (lineReader.TryReadLine(out var line))
                {
                    trimWriter.Write(Constants.NewLineMemory.Span);
                    if (!line.IsEmpty && line.Length >= commonIndent.Value)
                    {
                        var trimmedLine = line.Slice(commonIndent.Value);
                        trimWriter.Write(trimmedLine);
                    }
                }

            var trimmedValue = trimWriter.WrittenSpan;
            var leadingWhiteSpace = LeadingWhiteSpace(trimmedValue);
            var trailingWhiteSpace = TrailingWhitespace(trimmedValue);
            var finalValue = trimmedValue
                .Slice(0, trimmedValue.Length - trailingWhiteSpace)
                .Slice(leadingWhiteSpace);

            return finalValue;
        }

        private int? CommonIndent(in ReadOnlySpan<byte> span)
        {
            int? commonIndent = null;
            var lineReader = new LineReader(in span);

            if (!lineReader.TryReadLine(out _)) throw new Exception("Could not read line starting");

            while (lineReader.TryReadLine(out var line))
            {
                var length = line.Length;
                var indent = LeadingWhiteSpace(line);

                if (indent < length)
                    if (commonIndent == null || indent < commonIndent)
                        commonIndent = indent;
            }

            return commonIndent;
        }

        private int LeadingWhiteSpace(in ReadOnlySpan<byte> line)
        {
            if (line.IsEmpty)
                return 0;

            var position = 0;
            while (position < line.Length)
            {
                if (!IsWhiteSpace(line[position])) break;
                position++;
            }

            return position;
        }

        private int TrailingWhitespace(in ReadOnlySpan<byte> line)
        {
            if (line.IsEmpty)
                return 0;

            var position = line.Length - 1;
            var count = 0;
            while (position > 0)
            {
                if (!IsWhiteSpace(line[position]))
                    break;

                position--;
                count++;
            }

            return count;
        }

        private bool IsWhiteSpace(in byte code)
        {
            return code switch
            {
                Constants.Tab => true,
                Constants.Space => true,
                Constants.Return => true,
                Constants.NewLine => true,
                _ => false
            };
        }
    }
}