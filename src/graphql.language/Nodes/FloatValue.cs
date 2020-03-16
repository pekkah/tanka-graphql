using System;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class FloatValue : IValue
    {
        public readonly ReadOnlyMemory<byte> Value;
        public readonly Location Location;

        public FloatValue(
            in ReadOnlySpan<byte> value,
            in Location location)
        {
            Value = new ReadOnlyMemory<byte>(value.ToArray());
            Location = location;
        }
    }
}