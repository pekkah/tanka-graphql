﻿namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class IntValue : IValue
    {
        public readonly int Value;
        public readonly Location? Location;

        public IntValue(
            in int value,
            in Location? location)
        {
            Value = value;
            Location = location;
        }
    }
}