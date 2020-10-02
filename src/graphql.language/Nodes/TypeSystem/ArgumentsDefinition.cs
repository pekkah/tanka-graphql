using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class ArgumentsDefinition : CollectionNodeBase<InputValueDefinition>
    {
        public ArgumentsDefinition(IReadOnlyList<InputValueDefinition> items, in Location? location = default) :
            base(items, in location)
        {
        }

        public override NodeKind Kind => NodeKind.ArgumentsDefinition;

        public static ArgumentsDefinition? From(IReadOnlyList<InputValueDefinition>? args)
        {
            if (args == null)
                return null;

            return new ArgumentsDefinition(args);
        }
    }
}