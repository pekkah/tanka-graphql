using System;

namespace tanka.graphql.type
{
    public class Argument
    {
        public IType Type { get; set; }

        public object DefaultValue { get; set; }

        public Meta Meta { get; set; }

        public Argument(IType type, object defaultValue = null, Meta meta = null)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            DefaultValue = defaultValue;
            Meta = meta ?? new Meta();
        }

        [Obsolete]
        public Argument()
        {
        }

        [Obsolete]
        public static Argument Arg(IType type, object defaultValue = null, Meta meta = null)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return new Argument(type, defaultValue, meta);
        }
    }
}