﻿using System.Collections.Generic;

namespace Tanka.GraphQL.Introspection;

// ReSharper disable once InconsistentNaming
public class __Directive
{
    public string Description { get; set; }

    public List<__DirectiveLocation> Locations { get; set; }
    public string Name { get; set; }
}