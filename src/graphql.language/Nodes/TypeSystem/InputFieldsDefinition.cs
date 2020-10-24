using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class InputFieldsDefinition: CollectionNodeBase<InputValueDefinition>
    {
        public InputFieldsDefinition(IReadOnlyList<InputValueDefinition> items, in Location? location = default) : base(items, in location)
        {
        }

        public override NodeKind Kind => NodeKind.InputFieldsDefinition;

        public static InputFieldsDefinition? From(IReadOnlyList<InputValueDefinition>? fields)
        {
            if (fields == null)
                return null;

            return new InputFieldsDefinition(fields);
        }
    }
}