using System;
using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes
{
    public class Document
    {
        public readonly IReadOnlyCollection<FragmentDefinition>? FragmentDefinitions;

        public readonly IReadOnlyCollection<OperationDefinition>? OperationDefinitions;

        public Document(
            in IReadOnlyCollection<OperationDefinition> operationDefinitions)
        {
            OperationDefinitions = operationDefinitions;
            FragmentDefinitions = default;
        }

        /*
        public readonly TypeSystemDefinition[] TypeSystemDefinitions
        public readonly TypeSystemExtension[] TypeSystemExtensions 
        */
    }

    public class VariableDefinition
    {
        public readonly Variable Variable;
        public readonly Location? Location;

        public VariableDefinition(
            in Variable variable,
            in Location? location)
        {
            Variable = variable;
            Location = location;
        }
    }

    public abstract class Value
    {

    }

    public sealed class EnumValue : Value
    {
        public readonly Name Value;
        public readonly Location? Location;

        public EnumValue(
            in Name value,
            in Location? location)
        {
            Value = value;
            Location = location;
        }

    }

    public sealed class ObjectValue : Value
    {
        public readonly IReadOnlyCollection<ObjectField> Fields;
        public readonly Location? Location;

        public ObjectValue(
            in IReadOnlyCollection<ObjectField> fields,
            in Location? location)
        {
            Fields = fields;
            Location = location;
        }
    }

    public sealed class ObjectField
    {
        public readonly Name Name;
        public readonly Value Value;
        public readonly Location? Location;

        public ObjectField(
            in Name name,
            in Value value,
            in Location? location)
        {
            Name = name;
            Value = value;
            Location = location;
        }
    }
}