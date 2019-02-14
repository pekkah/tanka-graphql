using System;
using System.Collections.Generic;
using tanka.graphql.type;

namespace tanka.graphql.graph
{
    public interface ISchemaVisitor
    {
        void VisitSchema(ISchema schema);

        void VisitObject(ObjectType query);

        void VisitField(ObjectType objectType, KeyValuePair<string, IField> field);

        void VisitUnion(UnionType unionType);

        void VisitDirectiveArgument(DirectiveType directiveType, KeyValuePair<string, Argument> argument);

        void VisitFieldArgument(ComplexType complexType, KeyValuePair<string, IField> field,
            KeyValuePair<string, Argument> argument);

        void VisitDirective(DirectiveType directiveType);

        void VisitEnum(EnumType enumType);

        void VisitField(InterfaceType interfaceType, KeyValuePair<string, IField> interfaceTypeField);

        void VisitInterface(InterfaceType interfaceType);

        void VisitInputObjectField(InputObjectType inputObjectType,
            KeyValuePair<string, InputObjectField> inputObjectField);

        void VisitInputObject(InputObjectType inputObjectType);

        void VisitScalar(ScalarType scalarType);

        void VisitNamedType(INamedType namedType);
    }

    /*public abstract class TransformSchemaVisitor : ISchemaVisitor
    {

        public void ObjectType VisitObjectType(ISchema schema, ObjectType objectType)
        {
            var newFields = new List<KeyValuePair<string, IField>>();
            var removedFields = new List<KeyValuePair<string, IField>>();

            var fieldsModified = false;

            foreach (var objectTypeField in objectType.Fields)
            {
                var field = VisitObjectTypeField(schema, objectType, objectTypeField);

                if (field.Equals(default))
                {
                    removedFields.Add(objectTypeField);
                    fieldsModified = true;
                }

                if (!field.Equals(objectTypeField))
                {
                    removedFields.Add(objectTypeField);
                    newFields.Add(field);
                    fieldsModified = true;
                }
            }

            if (fieldsModified)
                return objectType
                    .ExcludeFields(removedFields)
                    .IncludeFields(newFields);

            return objectType;
        }
    }

    public class SchemaHealer2 : SchemaVisitor
    {
        public override KeyValuePair<string, IField> VisitObjectTypeField(
            ISchema schema,
            ObjectType objectType,
            KeyValuePair<string, IField> field)
        {
            if (field.Value.Type.Unwrap() is NamedTypeReference typeReference)
            {
                var namedType = schema.GetNamedType(typeReference.TypeName);

                if (namedType == null)
                    throw new InvalidOperationException(
                        "Failed to heal schema. Could not build named type for field" +
                        $"{objectType}:{field.Key} from reference " +
                        $"{typeReference.TypeName}");

                var maybeWrappedType = WrapIfRequired(field.Value.Type, namedType);
                return field.WithType(maybeWrappedType);
            }

            return field;
        }

        private IType WrapIfRequired(IType currentType, IType namedType)
        {
            if (currentType is NonNull nonNull)
                return new NonNull(WrapIfRequired(nonNull.WrappedType, namedType));

            if (currentType is List list)
                return new List(WrapIfRequired(list.WrappedType, namedType));

            return namedType;
        }
    }
    */
}