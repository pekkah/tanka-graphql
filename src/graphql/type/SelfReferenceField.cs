namespace tanka.graphql.type
{
    public class SelfReferenceField : Field
    {
        public SelfReferenceField(IType templateType, Args arguments = null, Meta meta = null, object defaultValue = null) 
            : base(templateType, arguments, meta, defaultValue)
        {
        }
    }
}