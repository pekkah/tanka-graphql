namespace tanka.graphql.type
{
    public class SelfReferenceField : Field
    {
        public SelfReferenceField(Args arguments = null, Meta meta = null, object defaultValue = null) 
            : base(null, arguments, meta, defaultValue)
        {
        }
    }
}