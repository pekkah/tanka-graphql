namespace tanka.graphql.introspection
{
    public class TypeRef
    {
        public string Name { get; set; }

        public TypeRef WrappedType { get; set; }

        public bool IsNonNull { get;set; }

        public bool IsList { get; set; }
    }
}