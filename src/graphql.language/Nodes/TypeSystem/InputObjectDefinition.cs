﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class InputObjectDefinition : TypeDefinition
    {
        public InputObjectDefinition(
            StringValue? description,
            in Name name,
            IReadOnlyCollection<Directive>? directives,
            IReadOnlyCollection<InputValueDefinition>? fields,
            in Location? location = default)
        {
            Description = description;
            Name = name;
            Directives = directives;
            Fields = fields;
            Location = location;
        }

        public StringValue? Description { get; }
        public Name Name { get; }
        public IReadOnlyCollection<Directive>? Directives { get; }
        public IReadOnlyCollection<InputValueDefinition>? Fields { get; }
        public Location? Location { get; }

        public static implicit operator InputObjectDefinition(string value)
        {
            var parser = Parser.Create(Encoding.UTF8.GetBytes(value));
            return parser.ParseInputObjectDefinition();
        }

        public static implicit operator InputObjectDefinition(in ReadOnlySpan<byte> value)
        {
            var parser = Parser.Create(value);
            return parser.ParseInputObjectDefinition();
        }

        public static implicit operator string(InputObjectDefinition value)
        {
            throw new NotImplementedException();
        }
    }
}