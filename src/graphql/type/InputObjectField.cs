using System;
using System.Collections.Generic;
using fugu.graphql.execution;

namespace fugu.graphql.type
{
    public class InputObjectField : IDirectives
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
            if (!Validations.IsInputType(type))
            {
                throw new ArgumentOutOfRangeException(
                    $"Wrapped type of list is not valid input type. Wrapped type: {type.Unwrap().Name}");
            }

            Type = type;
            Meta = meta ?? new Meta();
            DefaultValue = defaultValue;
        }

        public InputObjectField(
            NonNull type,
            Meta meta = null,
            object defaultValue = null)
        {
            if (!Validations.IsInputType(type))
            {
                throw new ArgumentOutOfRangeException(
                    $"Wrapped type of NonNull is not valid input type. Wrapped type: {type.Unwrap().Name}");
            }

            Type = type;
            Meta = meta ?? new Meta();
            DefaultValue = defaultValue;
        }

        public object DefaultValue { get; set; }

        public Meta Meta { get; set; }

        public IGraphQLType Type { get; }

        public IEnumerable<DirectiveInstance> Directives => Meta.Directives;

        public DirectiveInstance GetDirective(string name)
        {
            return Meta.GetDirective(name);
        }
    }
}