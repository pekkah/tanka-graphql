namespace tanka.graphql.type
{
    public class InputObjectType : INamedType, IDescribable
    {
        public InputObjectType(string name, Meta meta = null)
        {
            Name = name;
            Meta = meta ?? new Meta(null);
        }

        public Meta Meta { get; }

        public string Description => Meta.Description;

        public string Name { get; }

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}