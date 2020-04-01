using System;
using System.Collections.Generic;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class TypeSystemDocument : INode
    {
        public TypeSystemDocument(
            IReadOnlyCollection<SchemaDefinition>? schemaDefinitions,
            IReadOnlyCollection<TypeDefinition>? typeDefinitions,
            IReadOnlyCollection<DirectiveDefinition>? directiveDefinitions,
            IReadOnlyCollection<SchemaExtension>? schemaExtensions,
            IReadOnlyCollection<TypeExtension>? typeExtensions)
        {
            SchemaDefinitions = schemaDefinitions;
            TypeDefinitions = typeDefinitions;
            DirectiveDefinitions = directiveDefinitions;
            SchemaExtensions = schemaExtensions;
            TypeExtensions = typeExtensions;
        }

        public IReadOnlyCollection<DirectiveDefinition>? DirectiveDefinitions { get; }

        public IReadOnlyCollection<SchemaDefinition>? SchemaDefinitions { get; }
        
        public IReadOnlyCollection<SchemaExtension>? SchemaExtensions { get; }
        
        public IReadOnlyCollection<TypeDefinition>? TypeDefinitions { get; }
        
        public IReadOnlyCollection<TypeExtension>? TypeExtensions { get; }
        
        public NodeKind Kind => NodeKind.TypeSystemDocument;
        
        public Location? Location => null;

        public static implicit operator TypeSystemDocument(string value)
        {
            var parser = Parser.Create(Encoding.UTF8.GetBytes(value));
            return parser.ParseTypeSystemDocument();
        }

        public static implicit operator TypeSystemDocument(in ReadOnlySpan<byte> value)
        {
            var parser = Parser.Create(value);
            return parser.ParseTypeSystemDocument();
        }
    }
}