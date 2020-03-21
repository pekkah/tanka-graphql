using System;
using System.Buffers;

namespace Tanka.GraphQL.Language.Internal
{
    public class BufferWriter: IDisposable
    {
        private readonly byte[] _buffer;
        private int _index;
        private readonly int _length;
        
        public BufferWriter(int length)
        {
            _buffer = ArrayPool<byte>.Shared.Rent(length);
            _length = length;
            _index = 0;
        }

        public void Write(in ReadOnlySpan<byte> span)
        {
            if (span.IsEmpty)
                return;

            var destination = FreeSpan.Slice(0, span.Length);
            span.CopyTo(destination);
            _index += span.Length;
        }

        public Span<byte> FreeSpan
        {
            get 
            { 
                Span<byte> span = _buffer;
                return span.Slice(_index, _length - WrittenSpan.Length);
            }
        }

        public ReadOnlySpan<byte> WrittenSpan
        {
            get
            {
                ReadOnlySpan<byte> span = _buffer;
                return span.Slice(0, _index);
            }
        }

        public void Dispose()
        {
            ArrayPool<byte>.Shared.Return(_buffer);
        }
    }
}