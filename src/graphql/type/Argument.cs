using System;
using System.Reflection;

namespace tanka.graphql.type
{
    public class Argument : IDescribable
    {
        public IType Type { get; set; }

        public object DefaultValue { get; set; }

        public Argument(IType type, object defaultValue = null, string description = null)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            DefaultValue = defaultValue;
            Description = description ?? string.Empty;
        }

        [Obsolete]
        public static Argument Arg(IType type, object defaultValue = null, string description = null)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return new Argument(type, defaultValue, description);
        }

        public string Description { get; }
    }
}