using System;
using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Language.Visitors
{
    public sealed class ExecutionDocumentWalkerOptions
    {
        public List<IVisit<ExecutableDocument>> ExecutableDocument { get; set; }
            = new List<IVisit<ExecutableDocument>>();

        public List<IVisit<FragmentDefinition>> FragmentDefinition { get; set; }
            = new List<IVisit<FragmentDefinition>>();

        public List<IVisit<OperationDefinition>> OperationDefinition { get; set; }
            = new List<IVisit<OperationDefinition>>();

        public List<IVisit<SelectionSet>> SelectionSet { get; set; }
            = new List<IVisit<SelectionSet>>();

        public List<IVisit<ISelection>> Selection { get; set; }
            = new List<IVisit<ISelection>>();

        public List< IVisit<FieldSelection>> FieldSelection { get; set; }
            = new List<IVisit<FieldSelection>>();

        public List<IVisit<InlineFragment>> InlineFragment { get; set; }
            = new List<IVisit<InlineFragment>>();

        public List<IVisit<FragmentSpread>> FragmentSpread { get; set; }
            = new List<IVisit<FragmentSpread>>();

        public List<IVisit<Argument>> Argument { get; set; }
            = new List<IVisit<Argument>>();

        public List<IVisit<NamedType>> NamedType { get; set; }
            = new List<IVisit<NamedType>>();

        public List<IVisit<VariableDefinition>> VariableDefinition { get; set; }
            = new List<IVisit<VariableDefinition>>();

        public List<IVisit<DefaultValue>> DefaultValue { get; set; }
            = new List<IVisit<DefaultValue>>();

        public List<IVisit<Value>> Value { get; set; }
            = new List<IVisit<Value>>();

        public List<IVisit<Directive>> Directive { get; set; }
            = new List<IVisit<Directive>>();

        public List<IVisit<TypeBase>> Type { get; set; }
            = new List<IVisit<TypeBase>>();

        public ExecutionDocumentWalkerOptions Add(object visitor)
        {
            if (visitor is IVisit<ExecutableDocument> ed)
                ExecutableDocument.Add(ed);

            if (visitor is IVisit<FragmentDefinition> fd)
                FragmentDefinition.Add(fd);

            if (visitor is IVisit<OperationDefinition> od)
                OperationDefinition.Add(od);

            if (visitor is IVisit<SelectionSet> ss)
                SelectionSet.Add(ss);

            
            if (visitor is IVisit<ISelection> s)
                Selection.Add(s);

            if (visitor is IVisit<FieldSelection> fs)
                FieldSelection.Add(fs);

            if (visitor is IVisit<InlineFragment> ift)
                InlineFragment.Add(ift);

            if (visitor is IVisit<FragmentSpread> fgs)
                FragmentSpread.Add(fgs);

            if (visitor is IVisit<Argument> arg)
                Argument.Add(arg);

            if (visitor is IVisit<NamedType> nt)
                NamedType.Add(nt);

            if (visitor is IVisit<VariableDefinition> vd)
                VariableDefinition.Add(vd);

            if (visitor is IVisit<DefaultValue> dv)
                DefaultValue.Add(dv);

            if (visitor is IVisit<Value> v)
                Value.Add(v);

            if (visitor is IVisit<Directive> d)
                Directive.Add(d);

            if (visitor is IVisit<TypeBase> t)
                Type.Add(t);

            return this;
        }
    }
}