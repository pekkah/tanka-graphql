using System;
using System.Linq;
using fugu.graphql.execution;

namespace fugu.graphql.type
{
    public class InputObjectType : ComplexType, IGraphQLType
    {
        public InputObjectType(string name, Fields fields, Meta meta = null)
        {
            Name = name;
            Meta = meta ?? new Meta(null);

            foreach (var field in fields)
            {
                var fieldType = field.Value.Type;
                var hasArguments = field.Value.Arguments.Any();
                if (hasArguments)
                    throw new InvalidOperationException(
                        $"Input type {name} cannot contain field {field.Key} with arguments");

                if (!Validations.IsInputType(fieldType))
                    throw new InvalidOperationException(
                        $"Input type {name} cannot contain a non input type field {field.Key}");

                AddField(field.Key, field.Value);
            }
        }

        public Meta Meta { get; }

        public override string Name { get; }

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}