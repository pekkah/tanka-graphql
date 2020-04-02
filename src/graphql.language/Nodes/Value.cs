﻿using System;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes
{
    public abstract class Value: INode
    {
        public static implicit operator Value(string value)
        {
            var parser = Parser.Create(Encoding.UTF8.GetBytes(value));
            return parser.ParseValue(true);
        }

        public static implicit operator Value(in ReadOnlySpan<byte> value)
        {
            var parser = Parser.Create(value);
            return parser.ParseValue(true);
        }

        public abstract NodeKind Kind { get; }
        public abstract Location? Location { get; }
    }
}