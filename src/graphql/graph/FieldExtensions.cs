using System.Collections.Generic;
using tanka.graphql.type;

namespace tanka.graphql.graph
{
    public static class FieldExtensions
    {
        public static KeyValuePair<string, IField> WithType(this KeyValuePair<string, IField> field, IType type)
        {
            return new KeyValuePair<string, IField>(
                field.Key,
                new Field(
                    type,
                    new Args(field.Value.Arguments),
                    field.Value.Meta));
        }
    }
}