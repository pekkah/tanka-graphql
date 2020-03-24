﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class EnumValueDefinition
    {
        public EnumValueDefinition(
            StringValue? description,
            EnumValue value,
            IReadOnlyCollection<Directive>? directives,
            in Location? location = default)
        {
            Description = description;
            Value = value;
            Directives = directives;
            Location = location;
        }

        public StringValue? Description { get; }
        public EnumValue Value { get; }
        public IReadOnlyCollection<Directive>? Directives { get; }
        public Location? Location { get; }

        public static implicit operator EnumValueDefinition(string value)
        {
            var parser = new Parser(Encoding.UTF8.GetBytes(value));
            return parser.ParseEnumValueDefinition();
        }

        public static implicit operator string(EnumValueDefinition value)
        {
            throw new NotImplementedException();
        }
    }
}