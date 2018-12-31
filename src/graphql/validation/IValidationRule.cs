namespace tanka.graphql.validation
{
    public interface IValidationRule
    {
        INodeVisitor CreateVisitor(ValidationContext context);
    }
}