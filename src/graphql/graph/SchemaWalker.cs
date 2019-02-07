using System.Collections.Generic;
using tanka.graphql.type;

namespace tanka.graphql.graph
{
    public class SchemaWalker
    {
        public SchemaWalker(ISchema schema)
        {
            Schema = schema;
        }

        public ISchema Schema { get; }

        public void Visit(ISchemaVisitor visitor)
        {
            visitor.VisitSchema(Schema);

            foreach (var directiveType in Schema.QueryDirectives())
            {
                VisitDirective(directiveType, visitor);

                foreach (var argument in directiveType.Arguments)
                    VisitDirectiveArgument(directiveType, argument, visitor);
            }

            foreach (var scalarType in Schema.QueryTypes<ScalarType>())
                VisitScalar(scalarType, visitor);

            foreach (var enumType in Schema.QueryTypes<EnumType>())
                VisitEnum(enumType, visitor);

            foreach (var inputObjectType in Schema.QueryTypes<InputObjectType>())
            {
                VisitInputObjectType(inputObjectType, visitor);

                foreach (var inputObjectField in inputObjectType.Fields)
                    VisitInputObjectField(inputObjectType, inputObjectField, visitor);
            }

            foreach (var interfaceType in Schema.QueryTypes<InterfaceType>())
            {
                VisitInterfaceType(interfaceType, visitor);

                foreach (var interfaceTypeField in interfaceType.Fields)
                {
                    VisitInterfaceField(interfaceType, interfaceTypeField, visitor);

                    foreach (var argument in interfaceTypeField.Value.Arguments)
                        VisitFieldArgument(interfaceType, interfaceTypeField, argument, visitor);
                }
            }

            foreach (var objectType in Schema.QueryTypes<ObjectType>())
            {
                VisitObjectType(objectType, visitor);

                foreach (var objectTypeField in objectType.Fields)
                {
                    VisitObjectField(objectType, objectTypeField, visitor);

                    foreach (var argument in objectTypeField.Value.Arguments)
                        VisitFieldArgument(objectType, objectTypeField, argument, visitor);
                }
            }

            foreach (var unionType in Schema.QueryTypes<UnionType>())
                VisitUnionType(unionType, visitor);
        }

        protected virtual void VisitUnionType(UnionType unionType, ISchemaVisitor visitor)
        {
            visitor.VisitNamedType(unionType);
            visitor.VisitUnion(unionType);
        }

        protected virtual void VisitObjectField(ObjectType objectType,
            KeyValuePair<string, IField> objectTypeField,
            ISchemaVisitor visitor)
        {
            visitor.VisitField(objectType, objectTypeField);
        }

        protected virtual void VisitObjectType(ObjectType objectType, ISchemaVisitor visitor)
        {
            visitor.VisitNamedType(objectType);
            visitor.VisitObject(objectType);
        }

        protected virtual void VisitDirectiveArgument(DirectiveType directiveType,
            KeyValuePair<string, Argument> argument, ISchemaVisitor visitor)
        {
            visitor.VisitDirectiveArgument(directiveType, argument);
        }

        protected virtual void VisitFieldArgument(ComplexType complexType, KeyValuePair<string, IField> field,
            KeyValuePair<string, Argument> argument, ISchemaVisitor visitor)
        {
            visitor.VisitFieldArgument(complexType, field, argument);
        }

        protected virtual void VisitDirective(DirectiveType directiveType, ISchemaVisitor visitor)
        {
            visitor.VisitDirective(directiveType);
        }

        protected virtual void VisitEnum(EnumType enumType, ISchemaVisitor visitor)
        {
            visitor.VisitNamedType(enumType);
            visitor.VisitEnum(enumType);
        }

        protected virtual void VisitInterfaceField(InterfaceType interfaceType,
            KeyValuePair<string, IField> interfaceTypeField, ISchemaVisitor visitor)
        {
            visitor.VisitField(interfaceType, interfaceTypeField);
        }

        protected virtual void VisitInterfaceType(InterfaceType interfaceType, ISchemaVisitor visitor)
        {
            visitor.VisitNamedType(interfaceType);
            visitor.VisitInterface(interfaceType);
        }

        protected virtual void VisitInputObjectField(InputObjectType inputObjectType,
            KeyValuePair<string, InputObjectField> inputObjectField, ISchemaVisitor visitor)
        {
            visitor.VisitInputObjectField(inputObjectType, inputObjectField);
        }

        protected virtual void VisitInputObjectType(InputObjectType inputObjectType, ISchemaVisitor visitor)
        {
            visitor.VisitNamedType(inputObjectType);
            visitor.VisitInputObject(inputObjectType);
        }

        protected virtual void VisitScalar(ScalarType scalarType, ISchemaVisitor visitor)
        {
            visitor.VisitNamedType(scalarType);
            visitor.VisitScalar(scalarType);
        }
    }
}