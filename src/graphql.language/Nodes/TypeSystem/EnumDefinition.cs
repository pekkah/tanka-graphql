﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class EnumDefinition : TypeDefinition
    {
        public EnumDefinition(
            StringValue? description,
            Name name,
            IReadOnlyCollection<Directive>? directives,
            IReadOnlyCollection<EnumValueDefinition>? values,
            in Location? location)
        {
            Description = description;
            Name = name;
            Directives = directives;
            Values = values;
            Location = location;
        }

        public StringValue? Description { get; }
        public Name Name { get; }
        public IReadOnlyCollection<Directive>? Directives { get; }
        public IReadOnlyCollection<EnumValueDefinition>? Values { get; }
        public Location? Location { get; }

        public static implicit operator EnumDefinition(string value)
        {
            var parser = new Parser(Encoding.UTF8.GetBytes(value));
            return parser.ParseEnumDefinition();
        }

        public static implicit operator string(EnumDefinition value)
        {
            throw new NotImplementedException();
        }
    }
}