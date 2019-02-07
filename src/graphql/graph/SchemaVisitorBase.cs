using System.Collections.Generic;
using tanka.graphql.type;

namespace tanka.graphql.graph
{
    public class SchemaVisitorBase : ISchemaVisitor
    {
        protected ISchema Schema { get; private set; }

        public virtual void VisitSchema(ISchema schema)
        {
            Schema = schema;
        }

        public virtual void VisitObject(ObjectType query)
        {
        }

        public virtual void VisitField(ObjectType objectType, KeyValuePair<string, IField> field)
        {
        }

        public virtual void VisitUnion(UnionType unionType)
        {
        }

        public virtual void VisitDirectiveArgument(DirectiveType directiveType, KeyValuePair<string, Argument> argument)
        {
        }

        public virtual void VisitFieldArgument(ComplexType complexType, KeyValuePair<string, IField> field,
            KeyValuePair<string, Argument> argument)
        {
        }

        public virtual void VisitDirective(DirectiveType directiveType)
        {
        }

        public virtual void VisitEnum(EnumType enumType)
        {
        }

        public virtual void VisitField(InterfaceType interfaceType, KeyValuePair<string, IField> interfaceTypeField)
        {
        }

        public virtual void VisitInterface(InterfaceType interfaceType)
        {
        }

        public virtual void VisitInputObjectField(InputObjectType inputObjectType,
            KeyValuePair<string, InputObjectField> inputObjectField)
        {
        }

        public virtual void VisitInputObject(InputObjectType inputObjectType)
        {
        }

        public virtual void VisitScalar(ScalarType scalarType)
        {
        }

        public virtual void VisitNamedType(INamedType namedType)
        {
        }
    }
}