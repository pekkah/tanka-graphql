﻿using System;
using System.Collections.Generic;
using tanka.graphql.execution;

namespace tanka.graphql.type
{
    public class InputObjectField : IDirectives, IDescribable
    {
        public InputObjectField(
            ScalarType type,
            Meta meta = null,
            object defaultValue = null)
        {
            Type = type;
            Meta = meta ?? new Meta();
            DefaultValue = defaultValue;
        }

        public InputObjectField(
            EnumType type,
            Meta meta = null,
            object defaultValue = null)
        {
            Type = type;
            Meta = meta ?? new Meta();
            DefaultValue = defaultValue;
        }

        public InputObjectField(
            InputObjectType type,
            Meta meta = null,
            object defaultValue = null)
        {
            Type = type;
            Meta = meta ?? new Meta();
            DefaultValue = defaultValue;
        }

        public InputObjectField(
            List type,
            Meta meta = null,
            object defaultValue = null)
        {
            if (!TypeIs.IsInputType(type))
                throw new ArgumentOutOfRangeException(
                    $"Wrapped type of list is not valid input type. Wrapped type: {type.Unwrap()}");

            Type = type;
            Meta = meta ?? new Meta();
            DefaultValue = defaultValue;
        }

        public InputObjectField(
            NonNull type,
            Meta meta = null,
            object defaultValue = null)
        {
            if (!TypeIs.IsInputType(type))
                throw new ArgumentOutOfRangeException(
                    $"Wrapped type of NonNull is not valid input type. Wrapped type: {type.Unwrap()}");

            Type = type;
            Meta = meta ?? new Meta();
            DefaultValue = defaultValue;
        }

        public object DefaultValue { get; set; }

        public Meta Meta { get; set; }

        public IType Type { get; }

        public string Description => Meta.Description;

        public IEnumerable<DirectiveInstance> Directives => Meta.Directives;

        public DirectiveInstance GetDirective(string name)
        {
            return Meta.GetDirective(name);
        }
    }
}