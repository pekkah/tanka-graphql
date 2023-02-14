namespace Tanka.GraphQL.Directives;

public class DirectiveVisitor
{
    public DirectiveNodeVisitor<DirectiveFieldVisitorContext>? FieldDefinition { get; set; }

    public DirectiveNodeVisitor<DirectiveTypeVisitorContext>? TypeDefinition { get; set; }
}