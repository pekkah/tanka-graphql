using System;
using System.Collections.Generic;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class TypeSystemDocument
    {
        public TypeSystemDocument(
            IReadOnlyCollection<SchemaDefinition>? schemaDefinitions,
            IReadOnlyCollection<TypeDefinition>? typeDefinitions,
            IReadOnlyCollection<DirectiveDefinition>? directiveDefinitions,
            IReadOnlyCollection<SchemaExtension>? schemaExtensions,
            IReadOnlyCollection<TypeDefinition>? typeExtensions)
        {
            SchemaDefinitions = schemaDefinitions;
            TypeDefinitions = typeDefinitions;
            DirectiveDefinitions = directiveDefinitions;
            SchemaExtensions = schemaExtensions;
            TypeExtensions = typeExtensions;
        }

        public IReadOnlyCollection<SchemaDefinition>? SchemaDefinitions { get; }
        public IReadOnlyCollection<TypeDefinition>? TypeDefinitions { get; }
        public IReadOnlyCollection<DirectiveDefinition>? DirectiveDefinitions { get; }
        public IReadOnlyCollection<SchemaExtension>? SchemaExtensions { get; }
        public IReadOnlyCollection<TypeDefinition>? TypeExtensions { get; }

        public static implicit operator TypeSystemDocument(string value)
        {
            var parser = new Parser(Encoding.UTF8.GetBytes(value));
            return parser.ParseTypeSystemDocument();
        }

        public static implicit operator string(TypeSystemDocument value)
        {
            throw new NotImplementedException();
        }
    }
}