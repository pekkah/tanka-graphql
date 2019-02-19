namespace tanka.graphql.type
{
    public static class SchemaExtensions
    {
        public static T GetNamedType<T>(this ISchema schema, string name) where T: INamedType
        {
            return (T) schema.GetNamedType(name);
        }
    }
}