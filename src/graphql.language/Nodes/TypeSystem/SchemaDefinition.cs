using System;
using System.Collections.Generic;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class SchemaDefinition
    {
        public SchemaDefinition(
            StringValue? description,
            IReadOnlyCollection<Directive>? directives,
            IReadOnlyCollection<(OperationType Operation, NamedType NamedType)> operations,
            in Location? location)
        {
            Description = description;
            Directives = directives;
            Operations = operations;
            Location = location;
        }

        public StringValue? Description { get; }
        public IReadOnlyCollection<Directive>? Directives { get; }
        public IReadOnlyCollection<(OperationType Operation, NamedType NamedType)> Operations { get; }
        public Location? Location { get; }

        public static implicit operator SchemaDefinition(string value)
        {
            var parser = new Parser(Encoding.UTF8.GetBytes(value));
            return parser.ParseSchemaDefinition();
        }

        public static implicit operator string(SchemaDefinition value)
        {
            throw new NotImplementedException();
        }
    }
}