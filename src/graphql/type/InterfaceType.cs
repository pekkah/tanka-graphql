﻿using System;
using System.Collections.Generic;

namespace tanka.graphql.type
{
    public class InterfaceType : ComplexType, IDirectives, IDescribable
    {
        public InterfaceType(string name, Fields fields, Meta meta = null)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));

            Name = name ?? throw new ArgumentNullException(nameof(name));

            Meta = meta ?? new Meta();

            foreach (var field in fields)
                AddField(field.Key, field.Value);
        }

        public Meta Meta { get; }
        public string Description => Meta.Description;

        public IEnumerable<DirectiveInstance> Directives => Meta.Directives;

        public DirectiveInstance GetDirective(string name)
        {
            return Meta.GetDirective(name);
        }

        public override string Name { get; }

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}