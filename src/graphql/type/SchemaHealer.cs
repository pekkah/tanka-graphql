using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using tanka.graphql.tools;

namespace tanka.graphql.type
{
    public class SchemaHealer : SchemaVisitorBase
    {
        public SchemaHealer(ISchema schema):base(schema)
        {
        }

        protected override Task VisitObjectFieldAsync(ObjectType objectType, KeyValuePair<string, IField> objectTypeField)
        {
            if (objectTypeField.Value.Type.Unwrap() is NamedTypeReference typeReference)
            {
                var namedType = Schema.GetNamedType(typeReference.TypeName);

                if (namedType == null)
                    throw new InvalidOperationException($"Failed to heal schema. Could not build named type for field" +
                                                        $"{objectType}:{objectTypeField.Key} from reference " +
                                                        $"{typeReference.TypeName}");
                         
                var maybeWrappedType = WrapIfRequired(objectTypeField.Value.Type, namedType);
                objectTypeField.Value.Type = maybeWrappedType;
            }

            return base.VisitObjectFieldAsync(objectType, objectTypeField);
        }

        private IGraphQLType WrapIfRequired(IGraphQLType currentType, IGraphQLType namedType)
        {
            if (currentType is NonNull nonNull)
            {
                return new NonNull(WrapIfRequired(nonNull.WrappedType, namedType));
            }

            if (currentType is List list)
            {
                return new List(WrapIfRequired(list.WrappedType, namedType));
            }

            return namedType;
        }
    }
}