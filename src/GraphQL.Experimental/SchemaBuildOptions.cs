namespace Tanka.GraphQL.Experimental
{
    public class SchemaBuildOptions
    {
        public bool BuildTypesFromOrphanedExtensions { get; set; } = false;
        public string? OverrideQueryRootName { get; set; }
        public string? OverrideMutationRootName { get; set; }
        public string? OverrideSubscriptionRootName { get; set; }
    }
}