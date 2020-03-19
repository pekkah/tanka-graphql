using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class SchemaExtension
    {
        public SchemaExtension(
            StringValue? description,
            IReadOnlyCollection<Directive>? directives,
            IReadOnlyCollection<(OperationType Operation, NamedType NamedType)>? operations,
            in Location? location)
        {
            Description = description;
            Directives = directives;
            Operations = operations;
            Location = location;
        }

        public StringValue? Description { get; }
        public IReadOnlyCollection<Directive>? Directives { get; }
        public IReadOnlyCollection<(OperationType Operation, NamedType NamedType)>? Operations { get; }
        public Location? Location { get; }
    }
}