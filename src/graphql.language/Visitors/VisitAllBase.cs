using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Language.Visitors
{
    public abstract class VisitAllBase :
        IVisit<ExecutableDocument>,
        IVisit<FragmentDefinition>,
        IVisit<OperationDefinition>,
        IVisit<SelectionSet>, 
        IVisit<ISelection>,
        IVisit<FieldSelection>,
        IVisit<InlineFragment>,
        IVisit<FragmentSpread>,
        IVisit<Argument>,
        IVisit<NamedType>,
        IVisit<VariableDefinition>,
        IVisit<DefaultValue>,
        IVisit<ValueBase>,
        IVisit<Directive>,
        IVisit<TypeBase>
    {
        public virtual void Enter(ExecutableDocument document)
        {
        }

        public virtual void Leave(ExecutableDocument document)
        {
        }

        public virtual void Enter(FragmentDefinition definition)
        {
        }

        public virtual void Leave(FragmentDefinition definition)
        {
        }

        public virtual void Enter(OperationDefinition definition)
        {
        }

        public virtual void Leave(OperationDefinition definition)
        {
        }

        public virtual void Enter(SelectionSet definition)
        {
        }

        public virtual void Leave(SelectionSet definition)
        {
        }

        public virtual void Enter(ISelection selection)
        {
        }

        public virtual void Enter(FieldSelection selection)
        {
        }

        public virtual void Enter(InlineFragment selection)
        {
        }

        public virtual void Enter(FragmentSpread selection)
        {
        }

        public virtual void Enter(Argument argument)
        {
        }

        public virtual void Enter(ValueBase value)
        {
        }

        public virtual void Enter(NamedType namedType)
        {
        }

        public virtual void Enter(Directive directive)
        {
        }

        public virtual void Enter(VariableDefinition definition)
        {
        }

        public virtual void Enter(DefaultValue defaultValue)
        {
        }

        public virtual void Enter(TypeBase type)
        {
        }

        public virtual void Leave(ISelection selection)
        {
        }

        public virtual void Leave(FieldSelection selection)
        {
        }

        public virtual void Leave(InlineFragment selection)
        {
        }

        public virtual void Leave(FragmentSpread selection)
        {
        }

        public virtual void Leave(Argument argument)
        {
        }

        public virtual void Leave(ValueBase value)
        {
        }

        public virtual void Leave(NamedType namedType)
        {
        }

        public virtual void Leave(Directive directive)
        {
        }

        public virtual void Leave(VariableDefinition definition)
        {
        }

        public virtual void Leave(DefaultValue defaultValue)
        {
        }

        public virtual void Leave(TypeBase defaultValue)
        {
        }
    }
}