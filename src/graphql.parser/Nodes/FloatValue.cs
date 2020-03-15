﻿namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class FloatValue : IValue
    {
        public readonly double Value;
        public readonly Location Location;

        public FloatValue(
            in double value,
            in Location location)
        {
            Value = value;
            Location = location;
        }
    }
}