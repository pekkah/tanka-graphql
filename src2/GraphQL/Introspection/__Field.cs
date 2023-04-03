using System.Collections.Generic;

namespace Tanka.GraphQL.Introspection;

// ReSharper disable once InconsistentNaming
public class __Field
{
    public List<__InputValue> Args { get; set; }

    public string DeprecationReason { get; set; }

    public string Description { get; set; }

    public bool IsDeprecated { get; set; }
    public string Name { get; set; }

    public __Type Type { get; set; }
}